using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECSBoids
{
    public class BoidSimulationAuthoring : MonoBehaviour
    {
        [Header("Simulation")]
        [SerializeField] private Vector3 simulationBounds;
        [SerializeField] private float simulationBoundsPadding = 1;
        [SerializeField] private uint numberOfBoidsToSpawn = 0;
        [SerializeField] private float spawnRadius = 10;
        [SerializeField] private GameObject boidPrefab;

        [Header("Boids Settings")]
        [SerializeField] private GameObjectBoids.BoidSettings boidSettings = new GameObjectBoids.BoidSettings();

        public class Baker : Baker<BoidSimulationAuthoring>
        {
            public override void Bake(BoidSimulationAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new BoidSimulation
                {
                    SimulationBounds = authoring.simulationBounds,
                    SimulationBoundsPadding = authoring.simulationBoundsPadding,
                    NumberOfBoidsToSpawn = authoring.numberOfBoidsToSpawn,
                    SpawnRadius = authoring.spawnRadius,
                    BoidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic),
                    MaxSpeed = authoring.boidSettings.MaxSpeed,
                    SeparationRange = authoring.boidSettings.SeparationRange,
                    SeparationFactor = authoring.boidSettings.SeparationFactor,
                    AlignmentRange = authoring.boidSettings.AlignmentRange,
                    AlignmentFactor = authoring.boidSettings.AlignmentFactor,
                    CohesionRange = authoring.boidSettings.CohesionRange,
                    CohesionFactor = authoring.boidSettings.CohesionFactor
                });
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, simulationBounds * 2);
        }
    }

    public partial struct BoidSimulation : IComponentData
    {
        public float3 SimulationBounds;
        public float SimulationBoundsPadding;
        public uint NumberOfBoidsToSpawn;
        public uint NumberOfBoidsSpawned;
        public float SpawnRadius;
        public Entity BoidPrefab;

        public float MaxSpeed;
        public float SeparationRange;
        public float SeparationFactor;

        public float AlignmentRange;
        public float AlignmentFactor;

        public float CohesionRange;
        public float CohesionFactor;
    }
}
