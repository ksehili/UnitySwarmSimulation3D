using Unity.Entities;
using UnityEngine;

namespace Sehili.Dots
{
    /// <summary>
    /// Acts as the bridge between the Unity Editor and the DOTS environment.
    /// Bakes the assigned values into pure Entity Component Data.
    /// </summary>
    public class SwarmAuthoring : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject BoidPrefab;
        public int SpawnCount = 5000;

        [Header("Movement")]
        public float MinSpeed = 2f;
        public float MaxSpeed = 5f;

        [Header("Perception")]
        public float PerceptionRadius = 2.5f;
        public float AvoidanceRadius = 1f;

        [Header("Steering Weights")]
        public float SeparationWeight = 1.5f;
        public float AlignmentWeight = 1.0f;
        public float CohesionWeight = 1.0f;

        [Header("Environment")]
        public float BoundsRadius = 15f;
        public float BoundsWeight = 3f;

        [Header("Predator")]
        public float PredatorAvoidanceRadius = 8f;
        public float PredatorAvoidanceWeight = 5f;

        /// <summary>
        /// The Baker class converts the MonoBehaviour data into Entity components during build or play mode.
        /// </summary>
        private class SwarmBaker : Baker<SwarmAuthoring>
        {
            public override void Bake(SwarmAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                // Bake the spawner configuration
                AddComponent(entity, new BoidSpawner
                {
                    Prefab = GetEntity(authoring.BoidPrefab, TransformUsageFlags.Dynamic),
                    SpawnCount = authoring.SpawnCount
                });

                // Bake the global simulation settings
                AddComponent(entity, new SwarmSettings
                {
                    MinSpeed = authoring.MinSpeed,
                    MaxSpeed = authoring.MaxSpeed,
                    PerceptionRadius = authoring.PerceptionRadius,
                    AvoidanceRadius = authoring.AvoidanceRadius,
                    SeparationWeight = authoring.SeparationWeight,
                    AlignmentWeight = authoring.AlignmentWeight,
                    CohesionWeight = authoring.CohesionWeight,
                    BoundsRadius = authoring.BoundsRadius,
                    BoundsWeight = authoring.BoundsWeight,
                    PredatorAvoidanceRadius = authoring.PredatorAvoidanceRadius,
                    PredatorAvoidanceWeight = authoring.PredatorAvoidanceWeight
                });

                // Initialize the predator singleton with a position far outside the bounds
                AddComponent(entity, new PredatorData
                {
                    Position = new Unity.Mathematics.float3(9999f, 9999f, 9999f)
                });
            }
        }
    }
}