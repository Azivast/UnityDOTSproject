using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

namespace ECSBoids
{

    public partial struct BoidSystem : ISystem
    {
        NativeList<RefRO<Boid>> boids;
        NativeList<RefRO<LocalTransform>> boidTransforms;

        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Boid>();

            boids = new NativeList<RefRO<Boid>>(Allocator.Persistent);
            boidTransforms = new NativeList<RefRO<LocalTransform>>(Allocator.Persistent);
            foreach (var (boid, localTransform) in SystemAPI.Query<RefRO<Boid>, RefRO<LocalTransform>>())
            {
                boids.Add(boid);
                boidTransforms.Add(localTransform);
            }
        }


        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            BoidJob boidJob = new BoidJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                boids = boids.AsArray(),
                boidTransforms = boidTransforms.AsArray()
            };

            boidJob.ScheduleParallel();

        }

        private void OnDestroy(ref SystemState state)
        {
            boids.Dispose();
            boidTransforms.Dispose();
        }
    }

    [BurstCompile]
    [WithAll(typeof(Boid))]
    public partial struct BoidJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public NativeArray<RefRO<Boid>> boids;
        [ReadOnly] public NativeArray<RefRO<LocalTransform>> boidTransforms;

        [BurstCompile]
        public void Execute(ref Boid currentBoid, ref LocalTransform localTransform)
        {
            float3 seperationVelocity = float3.zero;
            float3 alignmentVelocity = float3.zero;
            float3 cohesionVelocity = float3.zero;
            float3 positionToMoveTowards = float3.zero;

            float numOfBoidsToAvoid = 0;
            float numOfBoidsAlignedWith = 0;
            float numOfBoidsInFlock = 0;

            //foreach (RefRO<Boid> otherBoid in boids)
            for (int i = 0; i < boids.Length; i++)
            {
                RefRO<Boid> otherBoid = boids[i];
                float3 otherBoidPosition = boidTransforms[i].ValueRO.Position;

                if (currentBoid.Equals(otherBoid.ValueRO)) continue; // skip itself

                float dist = math.distance(localTransform.Position, otherBoidPosition);

                // Separation
                if (dist < currentBoid.SeparationRange)
                {
                    float3 distanceVector = localTransform.Position - otherBoidPosition;
                    float3 travelDirection = math.normalize(distanceVector);
                    float3 weightedVelocity = travelDirection / dist;
                    seperationVelocity += weightedVelocity;
                    numOfBoidsToAvoid++;
                }

                // Alignment
                if (dist < currentBoid.AlignmentRange)
                {
                    alignmentVelocity += otherBoid.ValueRO.Velocity;
                    numOfBoidsAlignedWith++;
                }

                // Cohesion
                if (dist < currentBoid.CohesionRange)
                {
                    positionToMoveTowards += otherBoidPosition;
                    numOfBoidsInFlock++;
                }
            }


            // Calc Separation
            if (numOfBoidsToAvoid > 0)
            {
                seperationVelocity /= (float)numOfBoidsToAvoid;
                seperationVelocity *= currentBoid.SeparationFactor;
            }
            // Calc Alignment
            if (numOfBoidsAlignedWith > 0)
            {
                alignmentVelocity /= (float)numOfBoidsAlignedWith;
                alignmentVelocity *= currentBoid.AlignmentFactor;
            }
            // Calc Cohesion
            if (numOfBoidsInFlock > 0)
            {
                positionToMoveTowards /= (float)numOfBoidsInFlock;
                Vector3 cohesionDirection = positionToMoveTowards - localTransform.Position;
                cohesionDirection.Normalize();
                cohesionVelocity = cohesionDirection * currentBoid.CohesionFactor;
            }

            // Apply
            currentBoid.Velocity += seperationVelocity;
            currentBoid.Velocity += alignmentVelocity;
            currentBoid.Velocity += cohesionVelocity;

            // Clamp speed
            currentBoid.Velocity = math.normalize(currentBoid.Velocity) * currentBoid.MaxSpeed;
            // Update pos
            // Clamp position within simulation bounds
            // TODO: make this prettier
            var pos = localTransform.Position;

            if (pos.x < -currentBoid.SimulationBounds.x) pos.x = currentBoid.SimulationBounds.x - currentBoid.SimulationBoundsPadding;
            else if (pos.x > currentBoid.SimulationBounds.x) pos.x = -currentBoid.SimulationBounds.x + currentBoid.SimulationBoundsPadding;

            if (pos.y < -currentBoid.SimulationBounds.y) pos.y = currentBoid.SimulationBounds.y - currentBoid.SimulationBoundsPadding;
            else if (pos.y > currentBoid.SimulationBounds.y) pos.y = -currentBoid.SimulationBounds.y + currentBoid.SimulationBoundsPadding;

            if (pos.z < -currentBoid.SimulationBounds.z) pos.z = currentBoid.SimulationBounds.z - currentBoid.SimulationBoundsPadding;
            else if (pos.z > currentBoid.SimulationBounds.z) pos.z = -currentBoid.SimulationBounds.z + currentBoid.SimulationBoundsPadding;

            localTransform.Position = pos;

            // Apply velocity
            localTransform.Position += currentBoid.Velocity * deltaTime;
            // EntityManager em;
            // em.SetComponentData()
            // Update rot
            quaternion end = quaternion.LookRotation(currentBoid.Velocity, math.up());

            //quaternion result=quaternion.sl
            localTransform.Rotation = end;
        }
    }
}

