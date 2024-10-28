using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameObjectBoids
{
	public class Boid : MonoBehaviour
	{
		[Header("These variables are updated at runtime \nand are only serialized for visualization")]
		public Vector3 Velocity;
		[SerializeField] private Vector3 separationVelocity;
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
			separationVelocity = Vector3.zero;
			alignmentVelocity = Vector3.zero;
			cohesionVelocity = Vector3.zero;
			positionToMoveTowards = Vector3.zero;

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
					Vector3 weightedVelocity = dist <= 0.001f ? Vector3.zero : travelDirection / dist; // Making sure not to divide by zero
					separationVelocity += weightedVelocity;
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
				separationVelocity /= (float)numOfBoidsToAvoid;
				separationVelocity *= settings.SeparationFactor;
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
				//cohesionDirection.Normalize();
				cohesionVelocity = cohesionDirection * settings.CohesionFactor;
			}

			// Apply
			Velocity += separationVelocity;
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
			Gizmos.DrawLine(transform.position, transform.position+separationVelocity);
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


		//----------------------------------------------------------------------
		// The below functions have been merged into a single foreach loop in
		// UpdateBoid() above. I have only left them here to make it easier to
		// understand the algorithms. - Olle
		//----------------------------------------------------------------------
		private void Separation(List<Boid> boids)
		{
			separationVelocity = Vector3.zero;
			numOfBoidsToAvoid = 0;

			foreach (Boid boid in boids)
			{
				if (this == boid) continue; // skip self

				Vector3 boidPosition = boid.transform.position;
				float dist = Vector3.Distance(transform.position, boidPosition);

				// Add separation force for boid if in range
				if (dist < settings.SeparationRange)
				{
					Vector3 distanceVector = transform.position - boidPosition;
					Vector3 travelDirection = distanceVector.normalized;
					// Making sure not to divide by zero
					Vector3 weightedVelocity = dist <= 0.001f ? Vector3.zero : travelDirection / dist; 
					separationVelocity += weightedVelocity;
					numOfBoidsToAvoid++;
				}
			}

			// Calc mean separation & apply weight
			if (numOfBoidsToAvoid > 0)
			{
				separationVelocity /= (float)numOfBoidsToAvoid;
				separationVelocity *= settings.SeparationFactor;
			}

			// Apply
			Velocity += separationVelocity;
		}

		private void Alignment(List<Boid> boids)
		{
			alignmentVelocity = Vector3.zero;
			numOfBoidsAlignedWith = 0;


			foreach (Boid boid in boids)
			{
				if (this == boid) continue; // skip self

				Vector3 boidPosition = boid.transform.position;
				float dist = Vector3.Distance(transform.position, boidPosition);

				// Add alignment force for boid if in range
				if (dist < settings.AlignmentRange)
				{
					alignmentVelocity += boid.Velocity;
					numOfBoidsAlignedWith++;
				}
			}

			// Calc mean alignment & apply weight
			if (numOfBoidsAlignedWith > 0)
			{
				alignmentVelocity /= (float)numOfBoidsAlignedWith;
				alignmentVelocity *= settings.AlignmentFactor;
			}

			// Apply
			Velocity += alignmentVelocity;
		}

		private void Cohesion(List<Boid> boids)
		{
			cohesionVelocity = Vector3.zero;
			positionToMoveTowards = Vector3.zero;

			numOfBoidsInFlock = 0;


			foreach (Boid boid in boids)
			{
				if (this == boid) continue; // skip self

				Vector3 boidPosition = boid.transform.position;
				float dist = Vector3.Distance(transform.position, boidPosition);

				// Add cohesion force for boid if in range
				if (dist < settings.CohesionRange)
				{
					positionToMoveTowards += boidPosition;
					numOfBoidsInFlock++;
				}
			}

			// Calc mean cohesion & apply weight
			if (numOfBoidsInFlock > 0)
			{
				positionToMoveTowards /= (float)numOfBoidsInFlock;
				Vector3 cohesionDirection = positionToMoveTowards - transform.position;
				cohesionVelocity = cohesionDirection * settings.CohesionFactor;
			}

			// Apply
			Velocity += cohesionVelocity;
		}
	}
}
