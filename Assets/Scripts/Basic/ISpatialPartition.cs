using System.Collections.Generic;
using UnityEngine;

namespace Sehili.Basic
{
    /// <summary>
    /// Defines the contract for spatial partitioning algorithms used in the swarm simulation.
    /// Allows easy swapping between implementations like Octree or Spatial Hashing.
    /// </summary>
    public interface ISpatialPartition
    {
        /// <summary>
        /// Clears the data structure. Must be called at the beginning of each frame.
        /// </summary>
        void Clear();

        /// <summary>
        /// Inserts a boid into the spatial partition data structure.
        /// </summary>
        /// <param name="boid">The boid to insert.</param>
        void Insert(Boid boid);

        /// <summary>
        /// Populates the provided list with neighboring boids within the specified radius.
        /// </summary>
        /// <param name="position">The center position of the search query.</param>
        /// <param name="radius">The radius of the search query.</param>
        /// <param name="results">The list to populate with results (reused to avoid GC allocations).</param>
        void GetNeighbors(Vector3 position, float radius, List<Boid> results);
    }
}