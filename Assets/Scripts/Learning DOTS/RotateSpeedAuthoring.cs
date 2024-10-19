using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class RotateSpeedAuthoring : MonoBehaviour
{
	public float speed;

	private class Baker : Baker<RotateSpeedAuthoring> 
	{
		public override void Bake(RotateSpeedAuthoring authoring) 
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new RotateSpeed 
			{
				speed = authoring.speed
			});
		}
	}
}

public struct RotateSpeed : IComponentData
{
	public float speed;
}
