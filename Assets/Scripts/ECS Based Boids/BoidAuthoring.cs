using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECSBoids
{
    public class BoidAuthoring : MonoBehaviour
    {

        public class Baker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Boid());
            }
        }
    }

    public partial struct Boid : IComponentData
    {
        public float3 Velocity;
        public float3 seperationVelocity;
        public float3 alignmentVelocity;
        public float3 cohesionVelocity;
        public float3 positionToMoveTowards;
        public int numOfBoidsToAvoid;
        public int numOfBoidsAlignedWith;
        public int numOfBoidsInFlock;

        public float3 SimulationBounds;
        public float SimulationBoundsPadding;

        public float MaxSpeed;
        public float SeparationRange;
        public float SeparationFactor;
        public float AlignmentRange;
        public float AlignmentFactor;
        public float CohesionRange;
        public float CohesionFactor;
    }
}