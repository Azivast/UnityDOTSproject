using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

namespace GameObjectBoids
{
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
		private float simulationBoundsPadding;
		[SerializeField] private BoidSettings settings;
		
		
		public void Constructor(BoidSettings settings, Vector3 simulationBounds, float simulationBoundsPadding)
		{
			this.settings = settings;
			this.simulationBounds = simulationBounds;
			this.simulationBoundsPadding = simulationBoundsPadding;

			Velocity = UnityEngine.Random.insideUnitSphere;
		}
		
		
		public void UpdateBoid(List<Boid> boids, float deltaTime)
		{
			seperationVelocity = Vector3.zero;
			alignmentVelocity = Vector3.zero;
			cohesionVelocity = Vector3.zero;
			
			numOfBoidsToAvoid = 0;
			numOfBoidsAlignedWith = 0;
			numOfBoidsInFlock = 0;
			

			foreach (Boid boid in boids)
			{
				if (this == boid) continue; // skip itself
				
				Vector3 boidPosition = boid.transform.position;
				float dist = Vector3.Distance(transform.position, boidPosition);
				
				// Separation
				if (dist < settings.SeparationRange)
				{
					Vector3 distanceVector = transform.position - boidPosition;
					Vector3 travelDirection = distanceVector.normalized;
					Vector3 weightedVelocity = travelDirection / dist;
					seperationVelocity += weightedVelocity;
					numOfBoidsToAvoid++;
				}

				// Alignment
				if (dist < settings.AlignmentRange)
				{	
					alignmentVelocity += boid.Velocity;
					numOfBoidsAlignedWith++;
				}

				// Cohesion
				if (dist < settings.CohesionRange)
				{
					positionToMoveTowards += boidPosition;
					numOfBoidsInFlock++;
				}
			}


			// Calc Separation
			if (numOfBoidsToAvoid > 0)
			{
				seperationVelocity /= (float)numOfBoidsToAvoid;
				seperationVelocity *= settings.SeparationFactor;
			}
			// Calc Alignment
			if (numOfBoidsAlignedWith > 0)
			{
				alignmentVelocity /= (float)numOfBoidsAlignedWith;
				alignmentVelocity *= settings.AlignmentFactor;
			}
			// Calc Cohesion
			if (numOfBoidsInFlock > 0)
			{
				positionToMoveTowards /= (float)numOfBoidsInFlock;
				Vector3 cohesionDirection = positionToMoveTowards - transform.position;
				cohesionDirection.Normalize();
				cohesionVelocity = cohesionDirection * settings.CohesionFactor;
			}

			// Apply
			Velocity += seperationVelocity;
			Velocity += alignmentVelocity;
			Velocity += cohesionVelocity;
			
			ClampBoidVelocity();
			UpdatePosition(deltaTime);
			UpdateRotation(deltaTime);
		}

		private void ClampBoidVelocity()
		{
			Velocity = Velocity.normalized * settings.MaxSpeed;
		}

		private void UpdateRotation(float deltaTime)
		{
			transform.LookAt(transform.position+Velocity);
		}

		private void UpdatePosition(float deltaTime)
		{
			// Clamp position within simulation bounds
			// TODO: make this prettier
			var pos = transform.position;
			
			if (pos.x < -simulationBounds.x) pos.x = simulationBounds.x-simulationBoundsPadding;
			else if (pos.x > simulationBounds.x) pos.x = -simulationBounds.x+simulationBoundsPadding;

			if (pos.y < -simulationBounds.y) pos.y = simulationBounds.y-simulationBoundsPadding;
			else if (pos.y > simulationBounds.y) pos.y = -simulationBounds.y+simulationBoundsPadding;

			if (pos.z < -simulationBounds.z) pos.z = simulationBounds.z-simulationBoundsPadding;
			else if (pos.z > simulationBounds.z) pos.z = -simulationBounds.z+simulationBoundsPadding;

			transform.position = pos;
			
			// Apply velocity
			transform.position += Velocity * deltaTime;
		}

		private void OnDrawGizmosSelected()
		{
			// draw direction
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(transform.position, transform.position+Velocity);


			// draw separation
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.position+seperationVelocity);
			Gizmos.DrawWireSphere(transform.position, settings.SeparationRange);
			// alignment
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position, transform.position+alignmentVelocity);
			Gizmos.DrawWireSphere(transform.position, settings.AlignmentRange);
			// cohesion
			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, transform.position+cohesionVelocity);
			Gizmos.DrawWireSphere(transform.position, settings.CohesionRange);
		}
	}
}
