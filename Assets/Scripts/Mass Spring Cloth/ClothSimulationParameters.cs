using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores all the variables that shape the simulation.
/// </summary>
[System.Serializable]
public class ClothSimulationParameters : SimulationParameters {

	public enum FlexionSimulation {
		Springs,
		FaceOrientation
	};

	[Header ("Cloth specific parameters")]
	public float nodeMass = 1.0f;
	public FlexionSimulation flexionSimulation = FlexionSimulation.Springs;
	public float tractionSpringStiffness = 1600.0f;
	public float flexionSpringStiffness = 160.0f;
	public float K = 0.0005f;	// Orientacion

}