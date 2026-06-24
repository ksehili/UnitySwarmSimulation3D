using Unity.Entities;
using Unity.Mathematics;

namespace Sehili.Dots
{
    /// <summary>
    /// Represents the dynamic data of a single boid.
    /// This component is updated every frame by the steering system.
    /// </summary>
    public struct Boid : IComponentData
    {
        public float3 Velocity;
    }

    /// <summary>
    /// A lightweight struct used to store boid data inside the spatial hash map.
    /// Prevents the need to look up component data via Entity references during the neighbor search.
    /// </summary>
    public struct BoidData
    {
        public float3 Position;
        public float3 Velocity;
    }

    /// <summary>
    /// Singleton component containing all global simulation parameters.
    /// </summary>
    public struct SwarmSettings : IComponentData
    {
        public float MinSpeed;
        public float MaxSpeed;
        public float PerceptionRadius;
        public float AvoidanceRadius;
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
        public float BoundsRadius;
        public float BoundsWeight;
        public float PredatorAvoidanceRadius;
        public float PredatorAvoidanceWeight;
    }

    /// <summary>
    /// Component attached to a spawner entity to define how many boids to create.
    /// </summary>
    public struct BoidSpawner : IComponentData
    {
        public Entity Prefab;
        public int SpawnCount;
    }

    /// <summary>
    /// Singleton component that holds the current position of the predator.
    /// </summary>
    public struct PredatorData : IComponentData
    {
        public float3 Position;
    }
}