using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct RotatingCubeSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
    	state.RequireForUpdate<RotateSpeed>();
    }
	
	[BurstCompile]
	private void OnUpdate(ref SystemState state)
	{
		state.Enabled = false;
		return;
		// Var is also valid
		// foreach (var (localTransform, rotateSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>())
		
		// foreach ((RefRW<LocalTransform> localTransform, RefRO<RotateSpeed> rotateSpeed) 
		// in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>()) 
		// {
		// 	localTransform.ValueRW = localTransform.ValueRO.RotateY(rotateSpeed.ValueRO.speed * SystemAPI.Time.DeltaTime);
		// }

		RotatingCubeJob rotatingCubeJob = new RotatingCubeJob
		{
			deltaTime = SystemAPI.Time.DeltaTime
		};

		rotatingCubeJob.ScheduleParallel();
	}

	[BurstCompile]
	[WithAll(typeof(RotatingCube))]
	public partial struct RotatingCubeJob : IJobEntity
	{
		public float deltaTime;
		
		public void Execute(ref LocalTransform localTransform, in RotateSpeed rotateSpeed)
		{
			localTransform = localTransform.RotateY(rotateSpeed.speed * deltaTime);
		}
	}
}
