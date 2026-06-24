using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sehili.Dots
{
    /// <summary>
    /// Handles the initial instantiation of the boid swarm.
    /// Runs only when a BoidSpawner component is present in the world.
    /// </summary>
    [BurstCompile]
    public partial struct BoidSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Ensure the system only runs if there is a spawner and settings available
            state.RequireForUpdate<BoidSpawner>();
            state.RequireForUpdate<SwarmSettings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            BoidSpawner spawner = SystemAPI.GetSingleton<BoidSpawner>();
            SwarmSettings settings = SystemAPI.GetSingleton<SwarmSettings>();

            // Disable the system from running again by removing the spawner component
            state.EntityManager.RemoveComponent<BoidSpawner>(SystemAPI.GetSingletonEntity<BoidSpawner>());

            // Allocate a native array to hold the instantiated entities
            NativeArray<Entity> instances = new NativeArray<Entity>(spawner.SpawnCount, Allocator.Temp);
            state.EntityManager.Instantiate(spawner.Prefab, instances);

            // Initialize random positions and velocities for all spawned boids
            var random = new Unity.Mathematics.Random(1234);

            foreach (Entity entity in instances)
            {
                float3 randomPos = random.NextFloat3Direction() * random.NextFloat(0f, settings.BoundsRadius * 0.5f);
                float3 randomVel = random.NextFloat3Direction() * settings.MaxSpeed;

                state.EntityManager.SetComponentData(entity, new LocalTransform
                {
                    Position = randomPos,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                state.EntityManager.SetComponentData(entity, new Boid
                {
                    Velocity = randomVel
                });
            }

            // Free the temporary memory allocation
            instances.Dispose();
        }
    }
}