using UnityEngine;

namespace Sehili.Basic
{
    /// <summary>
    /// Contains all configurable parameters for the swarm simulation.
    /// Extracted into a ScriptableObject for easy tweaking at runtime without recompiling.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoidSettings", menuName = "Swarm/Boid Settings")]
    public class BoidSettings : ScriptableObject
    {
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

        [Header("Environment Constraints")]
        public float BoundsRadius = 15f;
        public float BoundsWeight = 3f;

        [Header("Predator Settings")]
        [Tooltip("Radius within which boids start avoiding the predator.")]
        public float PredatorAvoidanceRadius = 8f;
        [Tooltip("Strength of the repulsive force from the predator.")]
        public float PredatorAvoidanceWeight = 3f;
    }
}