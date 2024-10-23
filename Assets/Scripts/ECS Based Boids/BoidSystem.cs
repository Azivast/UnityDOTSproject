using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

namespace ECSBoids
{

    public partial struct BoidSystem : ISystem
    {

        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Boid>();
        }

        public void OnDestroy(ref SystemState state)
        {
            // Cleanup code if needed
        }

        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var world = state.WorldUnmanaged;
            var boidQuery = SystemAPI.QueryBuilder().WithAll<Boid>().WithAllRW<LocalToWorld>().Build();
            int boidCount = boidQuery.CalculateEntityCount();


            var boidPositions = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref world.UpdateAllocator);
            var boidVelocities = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref world.UpdateAllocator);

            //  Get positions and velocity of all boids.
            var getDataJob = new BoidsGetDataJob
            {
                Positions = boidPositions,
                Velocities = boidVelocities
            };
            JobHandle getDataJobHandle = getDataJob.ScheduleParallel(boidQuery, state.Dependency);
            state.Dependency = getDataJobHandle;
            getDataJobHandle.Complete();

            //  Update Boid position based on the data
            var updateBoidJob = new BoidUpdateJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                boidPositions = boidPositions,
                boidVelocities = boidVelocities
            };
            JobHandle updateBoidJobHandle = updateBoidJob.ScheduleParallel(boidQuery, state.Dependency);
            state.Dependency = updateBoidJobHandle;
            updateBoidJobHandle.Complete();

            boidPositions.Dispose();
            boidVelocities.Dispose();
        }
    }

    [BurstCompile]
    public partial struct BoidsGetDataJob : IJobEntity
    {
        public NativeArray<float3> Positions;
        public NativeArray<float3> Velocities;
        void Execute([EntityIndexInQuery] int entityIndexInQuery, in LocalToWorld localToWorld, in Boid boid)
        {
            Positions[entityIndexInQuery] = localToWorld.Position;
            Velocities[entityIndexInQuery] = boid.Velocity;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Boid))]
    public partial struct BoidUpdateJob : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public NativeArray<float3> boidPositions;
        [ReadOnly] public NativeArray<float3> boidVelocities;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndexInQuery, ref LocalToWorld localToWorld, ref Boid boid)
        {
            float3 separationVelocity = float3.zero;
            float3 alignmentVelocity = float3.zero;
            float3 cohesionVelocity = float3.zero;
            float3 positionToMoveTowards = float3.zero;
            //float3 velocityVector = float3.zero;
            float3 velocityVector = boidVelocities[boidIndexInQuery];

            float numOfBoidsToAvoid = 0;
            float numOfBoidsAlignedWith = 0;
            float numOfBoidsInFlock = 0;

            for (int otherBoidIndex = 0; otherBoidIndex < boidPositions.Length; otherBoidIndex++)
            {
                if (boidIndexInQuery == otherBoidIndex) continue; // skip itself

                float dist = math.distance(boidPositions[boidIndexInQuery], boidPositions[otherBoidIndex]);

                // Separation
                if (dist < boid.SeparationRange)
                {
                    
                    float3 distanceVector = boidPositions[boidIndexInQuery] - boidPositions[otherBoidIndex];
                    float3 travelDirection = math.normalizesafe(distanceVector);
                    float3 weightedVelocity = dist <= 0.001f ? float3.zero : travelDirection / dist; // Making sure not to divide by zero
                    separationVelocity += weightedVelocity;
                    numOfBoidsToAvoid++;
                }

                // Alignment
                if (dist < boid.AlignmentRange)
                {
                    alignmentVelocity += boidVelocities[otherBoidIndex];
                    numOfBoidsAlignedWith++;
                }

                // Cohesion
                if (dist < boid.CohesionRange)
                {
                    positionToMoveTowards += boidPositions[otherBoidIndex];
                    numOfBoidsInFlock++;
                }
            }


            // Calc Separation
            if (numOfBoidsToAvoid > 0)
            {
                separationVelocity /= (float)numOfBoidsToAvoid;
                separationVelocity *= boid.SeparationFactor;
            }
            // Calc Alignment
            if (numOfBoidsAlignedWith > 0)
            {
                alignmentVelocity /= (float)numOfBoidsAlignedWith;
                alignmentVelocity *= boid.AlignmentFactor;
            }
            // Calc Cohesion
            if (numOfBoidsInFlock > 0)
            {
                positionToMoveTowards /= (float)numOfBoidsInFlock;
                float3 cohesionDirection = positionToMoveTowards - boidPositions[boidIndexInQuery];
                cohesionDirection = math.normalizesafe(cohesionDirection);
                cohesionVelocity = cohesionDirection * boid.CohesionFactor;
            }

            // Apply
            velocityVector += separationVelocity;
            velocityVector += alignmentVelocity;
            velocityVector += cohesionVelocity;

            // Clamp speed
            velocityVector = math.normalizesafe(velocityVector) * boid.MaxSpeed;

            // Update pos
            // Clamp position within simulation bounds
            // TODO: make this prettier
            var newPos = boidPositions[boidIndexInQuery];

            if (newPos.x < -boid.SimulationBounds.x) newPos.x = boid.SimulationBounds.x - boid.SimulationBoundsPadding;
            else if (newPos.x > boid.SimulationBounds.x) newPos.x = -boid.SimulationBounds.x + boid.SimulationBoundsPadding;

            if (newPos.y < -boid.SimulationBounds.y) newPos.y = boid.SimulationBounds.y - boid.SimulationBoundsPadding;
            else if (newPos.y > boid.SimulationBounds.y) newPos.y = -boid.SimulationBounds.y + boid.SimulationBoundsPadding;

            if (newPos.z < -boid.SimulationBounds.z) newPos.z = boid.SimulationBounds.z - boid.SimulationBoundsPadding;
            else if (newPos.z > boid.SimulationBounds.z) newPos.z = -boid.SimulationBounds.z + boid.SimulationBoundsPadding;



            localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(
                new float3(newPos + (velocityVector * DeltaTime)),
                quaternion.LookRotationSafe(velocityVector, math.up()),
                new float3(1.0f, 1.0f, 1.0f))
            };

            boid.Velocity = velocityVector;
        }
    }
}

