using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;


namespace ECSBoids
{
    public partial struct BoidSimulationSystem : ISystem
    {
        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidSimulation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            // TODO: The following implementation might be slow since it checks each frame.
            // improve by only running once - OnCreate doesn't find BoidSimulation though.
            var query = SystemAPI.QueryBuilder().WithAll<BoidSimulation>().Build();
            if (query.CalculateEntityCount() > 0)
            {
                Entity simulationEntity = query.GetSingletonEntity();

                RefRW<BoidSimulation> simulation = SystemAPI.GetComponentRW<BoidSimulation>(simulationEntity);

                if (simulation.ValueRO.NumberOfBoidsSpawned >= simulation.ValueRO.NumberOfBoidsToSpawn)
                {
                    return;
                }

                EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);


                for (int i = 0; i < simulation.ValueRO.NumberOfBoidsToSpawn; i++)
                {
                    Entity newBoid = ecb.Instantiate(simulation.ValueRO.BoidPrefab);

                    ecb.AddComponent(newBoid, new Boid
                    {
                        Velocity = Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime / SystemAPI.Time.DeltaTime)).NextFloat3(), // TODO: This generate the same value for all boids
                        SimulationBounds = simulation.ValueRO.SimulationBounds,
                        SimulationBoundsPadding = simulation.ValueRO.SimulationBoundsPadding,
                        MaxSpeed = simulation.ValueRO.MaxSpeed,
                        SeparationRange = simulation.ValueRO.SeparationRange,
                        SeparationFactor = simulation.ValueRO.SeparationFactor,
                        AlignmentRange = simulation.ValueRO.AlignmentRange,
                        AlignmentFactor = simulation.ValueRO.AlignmentFactor,
                        CohesionRange = simulation.ValueRO.CohesionRange,
                        CohesionFactor = simulation.ValueRO.CohesionFactor,
                    });

                    simulation.ValueRW.NumberOfBoidsSpawned++;
                }
                ecb.Playback(state.EntityManager);
            }

        }
    }
}
