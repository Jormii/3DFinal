using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that simulates the behaviour of a spring.
/// </summary>
public class RigidBodySpring : Spring {

	public float volume;

	/// <summary>
	/// Initializes a Spring instance.
	/// </summary>
	/// <param name="nodeA">
	/// One of the nodes that make up the spring.
	/// </param>
	/// <param name="nodeB">
	/// The other node that makes up the spring.
	/// </param>
	/// <param name="volume">
	/// The volume of this spring.
	/// </param>
	/// <param name="simParams">
	/// A reference to the instance that holds the simulation parameters.
	/// </param>
	public RigidBodySpring (RigidBodyNode nodeA, RigidBodyNode nodeB, float volume, RigidBodySimulationParameters simParams):
		base (nodeA, nodeB, simParams) {
			this.volume = volume;
		}

	/// <summary>
	/// Calculates the forces affecting this spring and applies them to its nodes.
	/// </summary>
	public override void ComputeForce () {
		float L = (nodeA.pos - nodeB.pos).magnitude;
		float L0 = length0;
		float V = volume;
		float k = ((RigidBodySimulationParameters) simParams).elasticEnergyDensity;

		Vector3 force = -V / Mathf.Pow (L0, 2.0f) * k * (L - L0) * (nodeA.pos - nodeB.pos) / L;

		nodeA.force += force;
		nodeB.force -= force;
	}

	/// <summary>
	/// Returns a string that represents the spring.
	/// </summary>
	/// <returns>
	/// Said string.
	/// </returns>
	public override string ToString () {
		float k = ((RigidBodySimulationParameters) simParams).elasticEnergyDensity;

		return string.Format ("RigidBodySpring with L0 = {0}, V = {1}, ε = {2}, connecting nodes\n\t{3}\n\t{4}",
			length0, volume, k, nodeA, nodeB);
	}

}