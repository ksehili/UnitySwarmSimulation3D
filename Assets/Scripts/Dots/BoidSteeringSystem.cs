using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sehili.Dots
{
    /// <summary>
    /// The core physics and logic system for the boid swarm.
    /// Utilizes the Burst compiler and C# Job System for maximum multithreaded performance.
    /// </summary>
    [BurstCompile]
    public partial struct BoidSteeringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SwarmSettings>();
            state.RequireForUpdate<PredatorData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SwarmSettings settings = SystemAPI.GetSingleton<SwarmSettings>();
            PredatorData predator = SystemAPI.GetSingleton<PredatorData>();
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Query how many boids currently exist
            EntityQuery boidQuery = SystemAPI.QueryBuilder().WithAll<Boid, LocalTransform>().Build();
            int boidCount = boidQuery.CalculateEntityCount();

            if (boidCount == 0) return;

            // Create a spatial hash map. It must be disposed after the jobs finish.
            var spatialHashMap = new NativeParallelMultiHashMap<int3, BoidData>(boidCount, Allocator.TempJob);

            // JOB 1: Populate the spatial hash map in parallel
            var populateJob = new PopulateHashMapJob
            {
                CellSize = settings.PerceptionRadius,
                HashMapWriter = spatialHashMap.AsParallelWriter()
            };
            var populateHandle = populateJob.ScheduleParallel(state.Dependency);

            // JOB 2: Calculate steering forces and apply movement in parallel
            var steeringJob = new BoidSteeringJob
            {
                Settings = settings,
                PredatorPosition = predator.Position,
                DeltaTime = deltaTime,
                HashMap = spatialHashMap
            };
            var steeringHandle = steeringJob.ScheduleParallel(populateHandle);

            // Ensure the hash map memory is freed once the steering job completes
            spatialHashMap.Dispose(steeringHandle);

            // Assign the final job handle to the system state
            state.Dependency = steeringHandle;
        }

        // --- JOBS ---

        /// <summary>
        /// Calculates the grid cell for each boid and inserts its data into the parallel hash map.
        /// </summary>
        [BurstCompile]
        public partial struct PopulateHashMapJob : IJobEntity
        {
            public float CellSize;
            public NativeParallelMultiHashMap<int3, BoidData>.ParallelWriter HashMapWriter;

            public void Execute(in LocalTransform transform, in Boid boid)
            {
                int3 cellKey = new int3(math.floor(transform.Position / CellSize));

                HashMapWriter.Add(cellKey, new BoidData
                {
                    Position = transform.Position,
                    Velocity = boid.Velocity
                });
            }
        }

        /// <summary>
        /// Reads from the hash map to find neighbors and calculates the separation, alignment, and cohesion forces.
        /// </summary>
        [BurstCompile]
        public partial struct BoidSteeringJob : IJobEntity
        {
            [ReadOnly] public SwarmSettings Settings;
            [ReadOnly] public NativeParallelMultiHashMap<int3, BoidData> HashMap;
            public float3 PredatorPosition;
            public float DeltaTime;

            public void Execute(ref LocalTransform transform, ref Boid boid)
            {
                float3 position = transform.Position;
                float3 velocity = boid.Velocity;
                float3 acceleration = float3.zero;

                float3 centerOfFlock = float3.zero;
                float3 averageVelocity = float3.zero;
                float3 separationForce = float3.zero;

                int neighborCount = 0;
                int separationCount = 0;

                // Determine the cell the current boid is in
                int3 centerCell = new int3(math.floor(position / Settings.PerceptionRadius));

                // Search the 3x3x3 grid around the boid
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            int3 targetCell = centerCell + new int3(x, y, z);

                            // Iterate through all boids in this specific cell
                            if (HashMap.TryGetFirstValue(targetCell, out BoidData otherBoid, out NativeParallelMultiHashMapIterator<int3> iterator))
                            {
                                do
                                {
                                    float3 offset = otherBoid.Position - position;
                                    float sqrDistance = math.lengthsq(offset);

                                    // Ensure it's within radius and not identical to itself
                                    if (sqrDistance < Settings.PerceptionRadius * Settings.PerceptionRadius && sqrDistance > 0.0001f)
                                    {
                                        centerOfFlock += otherBoid.Position;
                                        averageVelocity += otherBoid.Velocity;
                                        neighborCount++;

                                        if (sqrDistance < Settings.AvoidanceRadius * Settings.AvoidanceRadius)
                                        {
                                            separationForce -= offset / sqrDistance;
                                            separationCount++;
                                        }
                                    }
                                } while (HashMap.TryGetNextValue(out otherBoid, ref iterator));
                            }
                        }
                    }
                }

                // Apply flocking rules if neighbors were found
                if (neighborCount > 0)
                {
                    centerOfFlock /= neighborCount;
                    averageVelocity /= neighborCount;

                    float3 cohesion = math.normalizesafe(centerOfFlock - position) * Settings.MaxSpeed - velocity;
                    float3 alignment = math.normalizesafe(averageVelocity) * Settings.MaxSpeed - velocity;

                    if (separationCount > 0)
                    {
                        separationForce /= separationCount;
                        separationForce = math.normalizesafe(separationForce) * Settings.MaxSpeed - velocity;
                    }

                    acceleration += separationForce * Settings.SeparationWeight;
                    acceleration += alignment * Settings.AlignmentWeight;
                    acceleration += cohesion * Settings.CohesionWeight;
                }

                // Predator Avoidance
                float distToPredator = math.distance(position, PredatorPosition);
                if (distToPredator < Settings.PredatorAvoidanceRadius)
                {
                    float3 repulsionDir = math.normalizesafe(position - PredatorPosition);
                    float falloff = 1f - (distToPredator / Settings.PredatorAvoidanceRadius);
                    acceleration += repulsionDir * (Settings.PredatorAvoidanceWeight * falloff);
                }

                // Boundary Constraints (Cage)
                float distanceToCenter = math.length(position);
                if (distanceToCenter > Settings.BoundsRadius)
                {
                    float3 directionToCenter = -math.normalizesafe(position);
                    acceleration += directionToCenter * (Settings.BoundsWeight * (distanceToCenter - Settings.BoundsRadius));
                }

                // Update Velocity and apply speed limits
                velocity += acceleration * DeltaTime;
                float speed = math.length(velocity);
                float3 dir = velocity / speed;
                speed = math.clamp(speed, Settings.MinSpeed, Settings.MaxSpeed);
                velocity = dir * speed;

                // Write back to components
                boid.Velocity = velocity;
                transform.Position += velocity * DeltaTime;

                // Safe rotation to prevent NaN errors when velocity is exactly zero
                if (math.lengthsq(velocity) > 0.001f)
                {
                    transform.Rotation = quaternion.LookRotationSafe(velocity, math.up());
                }
            }
        }
    }
}