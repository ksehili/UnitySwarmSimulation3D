using System.Collections.Generic;
using UnityEngine;

namespace Sehili.Basic
{
    /// <summary>
    /// A dictionary-based spatial hashing implementation to optimize neighbor searches.
    /// Divides 3D space into discrete cells to avoid O(N^2) complexity.
    /// </summary>
    public class SpatialHashGrid : ISpatialPartition
    {
        // --- Private Fields ---
        private readonly float _cellSize;
        private readonly Dictionary<Vector3Int, List<Boid>> _cells;

        // --- Constructor ---
        public SpatialHashGrid(float cellSize)
        {
            _cellSize = cellSize;
            // Pre-allocate dictionary capacity to avoid reallocations
            _cells = new Dictionary<Vector3Int, List<Boid>>(1000);
        }

        // --- Public Methods ---

        public void Clear()
        {
            // We clear the lists instead of clearing the dictionary to reuse allocated memory
            foreach (var cellList in _cells.Values)
            {
                cellList.Clear();
            }
        }

        public void Insert(Boid boid)
        {
            Vector3Int cellKey = GetCellKey(boid.Position);

            if (!_cells.TryGetValue(cellKey, out List<Boid> cellList))
            {
                cellList = new List<Boid>(20);
                _cells[cellKey] = cellList;
            }

            cellList.Add(boid);
        }

        public void GetNeighbors(Vector3 position, float radius, List<Boid> results)
        {
            Vector3Int centerCell = GetCellKey(position);
            int cellsToSearch = Mathf.CeilToInt(radius / _cellSize);

            // Loop through the center cell and surrounding cells
            for (int x = -cellsToSearch; x <= cellsToSearch; x++)
            {
                for (int y = -cellsToSearch; y <= cellsToSearch; y++)
                {
                    for (int z = -cellsToSearch; z <= cellsToSearch; z++)
                    {
                        Vector3Int neighborCell = centerCell + new Vector3Int(x, y, z);

                        if (_cells.TryGetValue(neighborCell, out List<Boid> cellList))
                        {
                            foreach (Boid otherBoid in cellList)
                            {
                                float sqrDistance = (position - otherBoid.Position).sqrMagnitude;

                                // Ensure it's within radius and not the exact same boid (distance > 0)
                                if (sqrDistance <= radius * radius && sqrDistance > 0f)
                                {
                                    results.Add(otherBoid);
                                }
                            }
                        }
                    }
                }
            }
        }

        // --- Private Methods ---

        /// <summary>
        /// Converts a 3D world position into a discrete grid coordinate.
        /// </summary>
        private Vector3Int GetCellKey(Vector3 position)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / _cellSize),
                Mathf.FloorToInt(position.y / _cellSize),
                Mathf.FloorToInt(position.z / _cellSize)
            );
        }
    }
}