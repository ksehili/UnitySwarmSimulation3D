using System.Collections.Generic;
using UnityEngine;

namespace Sehili.Basic
{
    /// <summary>
    /// Represents a single boid agent in the swarm simulation.
    /// Handles its own steering behavior calculations based on provided neighbors.
    /// </summary>
    public class Boid : MonoBehaviour
    {
        // --- Private Fields ---
        private Transform _cachedTransform;

        // --- Properties ---
        public Vector3 Velocity { get; private set; }
        public Vector3 Position => _cachedTransform.position;

        // --- Public Methods ---

        /// <summary>
        /// Initializes the boid with a starting velocity and caches necessary components.
        /// </summary>
        /// <param name="startVelocity">The initial velocity vector.</param>
        public void Initialize(Vector3 startVelocity)
        {
            _cachedTransform = transform;
            Velocity = startVelocity;
        }

        /// <summary>
        /// Calculates and applies the steering behaviors (Separation, Alignment, Cohesion)
        /// based on the provided neighbors and settings.
        /// </summary>
        /// <param name="settings">The swarm configuration parameters.</param>
        /// <param name="neighbors">The list of nearby boids.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public void UpdateMotion(BoidSettings settings, List<Boid> neighbors, Vector3 predatorPos, float deltaTime)
        {
            Vector3 acceleration = Vector3.zero;

            if (neighbors.Count > 0)
            {
                Vector3 centerOfFlock = Vector3.zero;
                Vector3 averageVelocity = Vector3.zero;
                Vector3 separationForce = Vector3.zero;
                int separationCount = 0;

                foreach (Boid neighbor in neighbors)
                {
                    Vector3 offset = neighbor.Position - Position;
                    float sqrDistance = offset.sqrMagnitude;

                    // Cohesion & Alignment accumulation
                    centerOfFlock += neighbor.Position;
                    averageVelocity += neighbor.Velocity;

                    // Separation accumulation (only apply if within avoidance radius)
                    if (sqrDistance < settings.AvoidanceRadius * settings.AvoidanceRadius && sqrDistance > 0f)
                    {
                        // The closer the neighbor, the stronger the repulsive force
                        separationForce -= offset / sqrDistance;
                        separationCount++;
                    }
                }

                centerOfFlock /= neighbors.Count;
                averageVelocity /= neighbors.Count;

                // Calculate the individual steering forces
                Vector3 cohesionForce = (centerOfFlock - Position).normalized * settings.MaxSpeed - Velocity;
                Vector3 alignmentForce = averageVelocity.normalized * settings.MaxSpeed - Velocity;

                if (separationCount > 0)
                {
                    separationForce /= separationCount;
                    separationForce = separationForce.normalized * settings.MaxSpeed - Velocity;
                }

                // Apply weighted forces to the overall acceleration
                acceleration += separationForce * settings.SeparationWeight;
                acceleration += alignmentForce * settings.AlignmentWeight;
                acceleration += cohesionForce * settings.CohesionWeight;
            }

            // Predator Avoidance (External Force)
            float distanceToPredator = Vector3.Distance(Position, predatorPos);

            // Only react if within the avoidance radius defined in ScriptableObject
            if (distanceToPredator < settings.PredatorAvoidanceRadius)
            {
                // Calculate repulsion vector (away from predator)
                Vector3 repulsionDir = (Position - predatorPos).normalized;

                // Calculate falloff: force is stronger when predator is closer (1.0 at distance 0, 0.0 at radius)
                float falloff = 1f - (distanceToPredator / settings.PredatorAvoidanceRadius);

                // Apply weighted force
                acceleration += repulsionDir * (settings.PredatorAvoidanceWeight * falloff);
            }

            // Boundary constraints: pull boids back to the center if they leave the allowed radius
            float distanceToCenter = Position.magnitude;
            if (distanceToCenter > settings.BoundsRadius)
            {
                Vector3 directionToCenter = -Position.normalized;
                acceleration += directionToCenter * (settings.BoundsWeight * (distanceToCenter - settings.BoundsRadius));
            }

            // Update velocity and clamp to min/max speeds
            Velocity += acceleration * deltaTime;
            float speed = Velocity.magnitude;
            Vector3 direction = Velocity / speed;
            speed = Mathf.Clamp(speed, settings.MinSpeed, settings.MaxSpeed);
            Velocity = direction * speed;

            // Apply movement and rotation
            _cachedTransform.position += Velocity * deltaTime;
            if (Velocity != Vector3.zero)
            {
                _cachedTransform.rotation = Quaternion.LookRotation(Velocity);
            }
        }
    }
}