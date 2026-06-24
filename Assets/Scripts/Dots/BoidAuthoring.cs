using Unity.Entities;
using UnityEngine;

namespace Sehili.Dots
{
    /// <summary>
    /// Attaches the Boid component data to the prefab during the baking process.
    /// This ensures the spawned entities have the necessary memory slots allocated 
    /// for the spawn system to inject the initial velocities.
    /// </summary>
    public class BoidAuthoring : MonoBehaviour
    {
        private class BoidBaker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                // Get the entity representation of the prefab
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add an empty Boid component to allocate the memory space
                AddComponent(entity, new Boid());
            }
        }
    }
}