using System;
using ECSBoids;
using Unity.Entities;
using UnityEngine;


namespace ECSBoids
{
    public class BoidObstacleAuthoringBaker : Baker<BoidObstacleAuthoring>
    {
        public override void Bake(BoidObstacleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new BoidObstacle());
        }
    }

    public struct BoidObstacle : IComponentData
    {
    }

    public class BoidObstacleAuthoring : MonoBehaviour
    {
    }
}