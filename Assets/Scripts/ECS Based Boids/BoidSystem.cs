using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECSBoids
{
    public partial struct BoidSystem : ISystem
    {
        EntityQuery otherBoidPositionsGroup;
        EntityQuery otherBoidVelocitiesGroup;

        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Boid>();
        }

        private void OnUpdate(ref SystemState state)
        {
            EntityQuery boidGroup = state.GetEntityQuery(typeof(Boid), ComponentType.ReadOnly<Boid>());

            BoidJob boidJob = new BoidJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
            };

            boidJob.ScheduleParallel(boidGroup);
        }
    }

    [BurstCompile]
    [WithAll(typeof(Boid))]
    public partial struct BoidJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public NativeArray<float3> otherBoidPositions;
        [ReadOnly] public NativeArray<float3> otherBoidVelocities;

        [BurstCompile]
        public void Execute(ref Boid currentBoid, ref LocalTransform localTransform)
        {
            // currentBoid.seperationVelocity = Vector3.zero;
            // currentBoid.alignmentVelocity = Vector3.zero;
            // currentBoid.cohesionVelocity = Vector3.zero;

            // currentBoid.numOfBoidsToAvoid = 0;
            // currentBoid.numOfBoidsAlignedWith = 0;
            // currentBoid.numOfBoidsInFlock = 0;

            // int i = -1;
            // foreach (float3 otherBoidPosition in otherBoidPositions)
            // {
            //     i++;
            //     if (localTransform.Equals(otherBoidPosition)) continue; // skip itself

            //     float dist = math.distance(localTransform.Position, otherBoidPosition);

            //     // Separation
            //     if (dist < currentBoid.SeparationRange)
            //     {
            //         float3 distanceVector = localTransform.Position - otherBoidPosition;
            //         float3 travelDirection = math.normalize(distanceVector);
            //         float3 weightedVelocity = travelDirection / dist;
            //         currentBoid.seperationVelocity += weightedVelocity;
            //         currentBoid.numOfBoidsToAvoid++;
            //     }

            //     // Alignment
            //     if (dist < currentBoid.AlignmentRange)
            //     {
            //         currentBoid.alignmentVelocity += otherBoidVelocities[i];
            //         currentBoid.numOfBoidsAlignedWith++;
            //     }

            //     // Cohesion
            //     if (dist < currentBoid.CohesionRange)
            //     {
            //         currentBoid.positionToMoveTowards += otherBoidPosition;
            //         currentBoid.numOfBoidsInFlock++;
            //     }
            // }


            // // Calc Separation
            // if (currentBoid.numOfBoidsToAvoid > 0)
            // {
            //     currentBoid.seperationVelocity /= (float)currentBoid.numOfBoidsToAvoid;
            //     currentBoid.seperationVelocity *= currentBoid.SeparationFactor;
            // }
            // // Calc Alignment
            // if (currentBoid.numOfBoidsAlignedWith > 0)
            // {
            //     currentBoid.alignmentVelocity /= (float)currentBoid.numOfBoidsAlignedWith;
            //     currentBoid.alignmentVelocity *= currentBoid.AlignmentFactor;
            // }
            // // Calc Cohesion
            // if (currentBoid.numOfBoidsInFlock > 0)
            // {
            //     currentBoid.positionToMoveTowards /= (float)currentBoid.numOfBoidsInFlock;
            //     Vector3 cohesionDirection = currentBoid.positionToMoveTowards - localTransform.Position;
            //     cohesionDirection.Normalize();
            //     currentBoid.cohesionVelocity = cohesionDirection * currentBoid.CohesionFactor;
            // }

            // // Apply
            // currentBoid.Velocity += currentBoid.seperationVelocity;
            // currentBoid.Velocity += currentBoid.alignmentVelocity;
            // currentBoid.Velocity += currentBoid.cohesionVelocity;

            // // Clamp speed
            // currentBoid.Velocity = math.normalize(currentBoid.Velocity) * currentBoid.MaxSpeed;
            // // Update pos
            // // Clamp position within simulation bounds
            // // TODO: make this prettier
            // var pos = localTransform.Position;

            // if (pos.x < -currentBoid.SimulationBounds.x) pos.x = currentBoid.SimulationBounds.x - currentBoid.SimulationBoundsPadding;
            // else if (pos.x > currentBoid.SimulationBounds.x) pos.x = -currentBoid.SimulationBounds.x + currentBoid.SimulationBoundsPadding;

            // if (pos.y < -currentBoid.SimulationBounds.y) pos.y = currentBoid.SimulationBounds.y - currentBoid.SimulationBoundsPadding;
            // else if (pos.y > currentBoid.SimulationBounds.y) pos.y = -currentBoid.SimulationBounds.y + currentBoid.SimulationBoundsPadding;

            // if (pos.z < -currentBoid.SimulationBounds.z) pos.z = currentBoid.SimulationBounds.z - currentBoid.SimulationBoundsPadding;
            // else if (pos.z > currentBoid.SimulationBounds.z) pos.z = -currentBoid.SimulationBounds.z + currentBoid.SimulationBoundsPadding;

            // localTransform.Position = pos;

            // // Apply velocity
            // localTransform.Position += currentBoid.Velocity * deltaTime;
            // // Update rot
            // quaternion end = quaternion.LookRotation(currentBoid.Velocity, math.up());

            // //quaternion result=quaternion.sl
            // localTransform.Rotation = end;
        }
    }
}

