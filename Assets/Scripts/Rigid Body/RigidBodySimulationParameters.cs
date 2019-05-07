using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores all the variables that shape the simulation.
/// </summary>
[System.Serializable]
public class RigidBodySimulationParameters : SimulationParameters {

	[Header ("Rigid body specific parameters")]
	public float density = 1.0f; // Changing this value during the simulation has no effect
	public float elasticEnergyDensity = 3200.0f;
	public bool debugTetrahedrons = false;

}