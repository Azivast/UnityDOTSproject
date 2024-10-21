using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

public class BoidSimulation : MonoBehaviour
{
	[Header("Simulation")]
	[SerializeField] private Vector3 simulationBounds;
	[SerializeField] private uint numberOfBoidsToSpawn = 0;
	[SerializeField] private float spawnRadius = 10;
	[SerializeField] private GameObject boidPrefab;

	[Header("Boids Settings")]
	[SerializeField] private BoidSettings boidSettings = new BoidSettings();

	[SerializeField] private List<Boid> boids;
	
	
	private void Start()
	{
		UnityEngine.Random.InitState(123);
		boids = new List<Boid>();
		SpawnBoids();
	}
	
	private void Update()
	{
		foreach(Boid boid in boids)
		{
			boid.UpdateBoid(boids, Time.deltaTime);
		}
	}
	
	private void SpawnBoids()
	{
		for(int i = 0; i < numberOfBoidsToSpawn; i++)
		{
			Boid boid = Instantiate(boidPrefab,
			transform.position + (Random.insideUnitSphere * spawnRadius),
			Random.rotation).GetComponent<Boid>();

			boid.Constructor(boidSettings, simulationBounds);

			boids.Add(boid);
		}
	}
	
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, spawnRadius);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, simulationBounds*2);
	}
}
