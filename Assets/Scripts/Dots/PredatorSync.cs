using Unity.Entities;
using UnityEngine;

namespace Sehili.Dots
{
    /// <summary>
    /// Synchronizes the position of a classic MonoBehaviour GameObject
    /// to the DOTS environment so the boids can react to it.
    /// </summary>
    public class PredatorSync : MonoBehaviour
    {
        private EntityManager _entityManager;
        private EntityQuery _predatorQuery;

        private void Start()
        {
            // Access the default DOTS world
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Create a query to find the singleton PredatorData component
            _predatorQuery = _entityManager.CreateEntityQuery(typeof(PredatorData));
        }

        private void Update()
        {
            if (_predatorQuery.HasSingleton<PredatorData>())
            {
                // Push the current GameObject transform position to the DOTS singleton
                _entityManager.SetComponentData(_predatorQuery.GetSingletonEntity(), new PredatorData
                {
                    Position = transform.position
                });
            }
            else
            {
                Debug.LogWarning("PredatorData singleton not found! The BoidSteeringSystem cannot see the predator. Check if SwarmAuthoring is inside the SubScene.");
            }
        }
    }
}