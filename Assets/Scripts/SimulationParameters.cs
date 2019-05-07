using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimulationParameters {

	public enum IntegrationMethod {
		ExplicitEuler,
		SymplecticEuler
	};

	[Header("General simulation parameters")]
	public bool pauseSimulation = false;
	public IntegrationMethod integrationMethod = IntegrationMethod.SymplecticEuler;
	public float timeStep = 0.002f;
	public int subSteps = 5;
	public Vector3 gravity = new Vector3 (0.0f, -9.81f, 0.0f);

	public float nodeDampingFactor = 0.5f;
	public float springDampingFactor = 0.0075f;

	public float windDamping = 0.1f;
	public Vector3 windVector = Vector3.zero;

	public bool debugNodes = false;
	public bool debugSprings = false;
	public bool debugTriangles = false;
}
