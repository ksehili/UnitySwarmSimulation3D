using System.Collections.Generic;
using UnityEngine;

namespace Sehili.Basic
{
    /// <summary>
    /// Orchestrates the swarm simulation, manages the lifecycle of all boids,
    /// and coordinates the spatial partitioning system.
    /// </summary>
    public class SwarmManager : MonoBehaviour
    {
        // --- Serialized Fields ---
        [SerializeField] private BoidSettings _settings;
        [Header("Prefabs")]
        [SerializeField] private Boid _simpleBoidPrefab;
        [SerializeField] private Boid _animatedBoidPrefab;
        [SerializeField] private bool _useAnimatedBoids = true;
        [SerializeField] private int _spawnCount = 500;

        [SerializeField] private PredatorController _predator;

        // --- Private Fields ---
        private List<Boid> _boids;
        private ISpatialPartition _spatialPartition;

        /// <summary>
        /// Pre-allocated buffer list to prevent garbage collection spikes during the neighbor search.
        /// </summary>
        private List<Boid> _neighborBuffer;

        // --- Unity Lifecycle Methods ---

        private void Start()
        {
            _boids = new List<Boid>(_spawnCount);
            _neighborBuffer = new List<Boid>(50);

            _spatialPartition = new SpatialHashGrid(_settings.PerceptionRadius);

            SpawnBoids();
        }

        private void Update()
        {
            if (_spatialPartition == null || _boids.Count == 0)
            {
                return;
            }

            // 1. Rebuild the spatial partition grid for the current frame
            _spatialPartition.Clear();
            foreach (Boid boid in _boids)
            {
                _spatialPartition.Insert(boid);
            }

            Vector3 predatorPosition = (_predator != null) ? _predator.Position : new Vector3(9999f, 9999f, 9999f);

            // 2. Calculate steering and update motion for all boids
            foreach (Boid boid in _boids)
            {
                _neighborBuffer.Clear();
                _spatialPartition.GetNeighbors(boid.Position, _settings.PerceptionRadius, _neighborBuffer);

                boid.UpdateMotion(_settings, _neighborBuffer, predatorPosition, Time.deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            if (_settings != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                Gizmos.DrawWireSphere(Vector3.zero, _settings.BoundsRadius);
            }
        }

        // --- Private Methods ---

        /// <summary>
        /// Instantiates the initial population of boids with random positions and velocities within the bounds.
        /// </summary>
        private void SpawnBoids()
        {
            Boid prefabToUse = _useAnimatedBoids ? _animatedBoidPrefab : _simpleBoidPrefab;
            for (int i = 0; i < _spawnCount; i++)
            {
                Vector3 randomPosition = Random.insideUnitSphere * (_settings.BoundsRadius * 0.5f);
                Vector3 randomVelocity = Random.insideUnitSphere * _settings.MaxSpeed;

                Boid newBoid = Instantiate(prefabToUse, randomPosition, Quaternion.identity, transform);
                newBoid.Initialize(randomVelocity);

                _boids.Add(newBoid);
            }
        }

        public void ResetSimulation()
        {
            // Destroy existing boids
            foreach (Boid boid in _boids)
            {
                if (boid != null) Destroy(boid.gameObject);
            }

            // Clear spatial partition and boid list
            _boids.Clear();
            _spatialPartition.Clear();

            // Respawn
            SpawnBoids();
        }
    }
}