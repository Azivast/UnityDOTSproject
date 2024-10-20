using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

[Serializable]
public class BoidSettings
{
	[Header("Speed")]
	public float MaxSpeed = 10;

	[Header("Separation")]
	public float SeparationRange = 10;
	public float SeparationFactor = 1;
	
	[Header("Alignment")]
	public float AlignmentRange = 10;
	public float AlignmentFactor = 1;
	
	[Header("Cohesion")]
	public float CohesionRange = 10;
	public float CohesionFactor = 1;

	public BoidSettings(){}
}
