using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that simulates the behaviour of a spring.
/// </summary>
public class ClothSpring : Spring {

	public bool isFlexionSpring;

	/// <summary>
	/// Initializes a Spring instance.
	/// </summary>
	/// <param name="nodeA">
	/// One of the nodes that make up the spring.
	/// </param>
	/// <param name="nodeB">
	/// The other node that makes up the spring.
	/// </param>
	/// <param name="isFlexionSpring">
	/// Whether the spring is a normal spring or a flexion one.
	/// </param>
	/// <param name="simParams">
	/// A reference to the instance that holds the simulation parameters.
	/// </param>
	public ClothSpring (Node nodeA, Node nodeB, bool isFlexionSpring, ClothSimulationParameters simParams):
		base (nodeA, nodeB, simParams) {
			this.isFlexionSpring = isFlexionSpring;
		}

	/// <summary>
	/// Calculates the forces affecting this spring and applies them to its nodes.
	/// </summary>
	public override void ComputeForce () {
		float tractionSpringStiffness = ((ClothSimulationParameters) simParams).tractionSpringStiffness;
		float flexionSpringStiffness = ((ClothSimulationParameters) simParams).flexionSpringStiffness;

		float stiffness = (isFlexionSpring) ? flexionSpringStiffness : tractionSpringStiffness;

		Vector3 u = nodeA.pos - nodeB.pos;
		float length = u.magnitude;
		u = u * (1.0f / length);

		Vector3 force = -stiffness * (length - length0) * u;
		force += -(simParams.springDampingFactor * stiffness) * (Vector3.Dot (u, nodeA.vel - nodeB.vel) * u);

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
		float tractionSpringStiffness = ((ClothSimulationParameters) simParams).tractionSpringStiffness;
		float flexionSpringStiffness = ((ClothSimulationParameters) simParams).flexionSpringStiffness;
		float stiffness = (isFlexionSpring) ? flexionSpringStiffness : tractionSpringStiffness;

		return string.Format ("Spring with L0 = {0}, k = {1}, Flexion = {2} connecting nodes:\n{3}\nAND\n{4}",
			length0, stiffness, isFlexionSpring, nodeA, nodeB);
	}

}