using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class Boid : MonoBehaviour
{
	[Header("These variables are updated at runtime \nand are only serialized for visualization")]
	public Vector3 Velocity;
	[SerializeField] private Vector3 seperationVelocity;
	[SerializeField] private Vector3 alignmentVelocity;
	[SerializeField] private Vector3 cohesionVelocity;
	[SerializeField] private Vector3 positionToMoveTowards;
	[SerializeField] private int numOfBoidsToAvoid;
	[SerializeField] private int numOfBoidsAlignedWith;
	[SerializeField] private int numOfBoidsInFlock;
	
	private Vector3 simulationBounds;
	[SerializeField] private BoidSettings settings;
	
	
	public void Constructor(BoidSettings settings, Vector3 simulationBounds)
	{
		this.settings = settings;
		this.simulationBounds = simulationBounds;

		Velocity = UnityEngine.Random.insideUnitSphere;
		seperationVelocity = Vector3.zero;
	}
	
	
	public void UpdateBoid(List<Boid> boids, float deltaTime)
	{
		seperationVelocity = Seperation(boids);
		alignmentVelocity = Alignment(boids);
		cohesionVelocity = Cohesion(boids);

		Velocity += seperationVelocity;
		Velocity += alignmentVelocity;
		Velocity += cohesionVelocity;
		
		ClampBoidVelocity();
		UpdatePosition(deltaTime);
		UpdateRotation(deltaTime);
		// DrawDebugView();
		// DrawDebugVies();
		
	}

	private void ClampBoidVelocity()
	{
		//if (Velocity.magnitude > settings.MaxSpeed);
		Velocity = Velocity.normalized * settings.MaxSpeed;
	}

	private void UpdateRotation(float deltaTime)
	{
		//throw new NotImplementedException();
	}

	private void UpdatePosition(float deltaTime)
	{
		transform.position += Velocity * deltaTime;
		
		// Clamp position within simulation bounds
		// TODO: make this prettier
		var pos = transform.position;
		if (pos.x < -simulationBounds.x) pos.x = simulationBounds.x;
		else if (pos.x > simulationBounds.x) pos.x = -simulationBounds.x;
		
		if (pos.y < -simulationBounds.y) pos.y = simulationBounds.y;
		else if (pos.y > simulationBounds.y) pos.y = -simulationBounds.y;
		
		if (pos.z < -simulationBounds.z) pos.z = simulationBounds.z;
		else if (pos.z > simulationBounds.z) pos.z = -simulationBounds.z;

		transform.position = pos;
	}

	private Vector3 Cohesion(List<Boid> boids)
	{
		numOfBoidsInFlock = 0;
		foreach (Boid boid in boids)
		{
			if (this == boid) continue; // skip itself

			Vector3 boidPosition = boid.transform.position;
			float dist = Vector3.Distance(transform.position, boidPosition);

			if (dist < settings.CohesionRange)
			{
				positionToMoveTowards += boidPosition;
				numOfBoidsInFlock++;
			}
		}

		if (numOfBoidsInFlock > 0)
		{
			positionToMoveTowards /= (float)numOfBoidsInFlock;
			Vector3 cohesionDirection = positionToMoveTowards - transform.position;
			cohesionDirection.Normalize();
			cohesionVelocity = cohesionDirection * settings.CohesionFactor;
		}
		
		return cohesionVelocity;
	}

	private Vector3 Alignment(List<Boid> boids)
	{
		numOfBoidsAlignedWith = 0;
		foreach (Boid boid in boids)
		{
			if (this == boid) continue; // skip itself

			Vector3 boidPosition = boid.transform.position;
			float dist = Vector3.Distance(transform.position, boidPosition);
			if (dist < settings.AlignmentRange)
			{	
				alignmentVelocity += boid.Velocity;
				numOfBoidsAlignedWith++;
			}
		}
		if (numOfBoidsAlignedWith > 0)
		{
			alignmentVelocity /= (float)numOfBoidsToAvoid;
			alignmentVelocity *= settings.AlignmentFactor;
		}
		
		return alignmentVelocity;
	}

	private Vector3 Seperation(List<Boid> boids)
	{
		numOfBoidsToAvoid = 0;
		foreach (Boid boid in boids)
		{
			if (this == boid) continue; // skip itself

			Vector3 boidPosition = boid.transform.position;
			float dist = Vector3.Distance(transform.position, boidPosition);
			if (dist < settings.SeparationRange)
			{
				Vector3 distanceVector = transform.position - boidPosition;
				Vector3 travelDirection = distanceVector.normalized;
				Vector3 weightedVelocity = travelDirection / dist;
				seperationVelocity += weightedVelocity;
				numOfBoidsToAvoid++;
			}
		}

		if (numOfBoidsToAvoid > 0)
		{
			seperationVelocity /= (float)numOfBoidsToAvoid;
			seperationVelocity *= settings.SeparationFactor;
		}

		return seperationVelocity;
	}
}
