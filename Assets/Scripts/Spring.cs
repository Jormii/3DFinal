using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spring {

	public Node nodeA;
	public Node nodeB;
	public float length0;
	public SimulationParameters simParams;

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
	public Spring (Node nodeA, Node nodeB, SimulationParameters simParams) {
		this.nodeA = nodeA;
		this.nodeB = nodeB;
		this.length0 = (nodeA.pos - nodeB.pos).magnitude;
		this.simParams = simParams;
	}

	/// <summary>
	/// Calculates the forces affecting this spring and applies them to its nodes.
	/// </summary>
	public abstract void ComputeForce ();

	/// <summary>
	/// Checks if two springs are equal.
	/// </summary>
	/// <remarks>
	/// Two springs are equals if the tuples of their nodes are the same.
	/// </remarks>
	/// <param name="obj">
	/// The spring to compare.
	/// </param>
	/// <returns>
	/// True if the springs meet the mentioned condition. False otherwise.
	/// </returns>
	public override bool Equals (object obj) {
		var otherSpring = obj as Spring;
		if (otherSpring == null) {
			return false;
		}

		return nodeA.Equals (otherSpring.nodeA) && nodeB.Equals (otherSpring.nodeB) ||
			nodeA.Equals (otherSpring.nodeB) && nodeB.Equals (otherSpring.nodeA);
	}

	/// <summary>
	/// A hash code for the current spring.
	/// </summary>
	/// <returns>
	/// The spring's hash code.
	/// </returns>
	public override int GetHashCode () {
		return nodeA.GetHashCode () * nodeB.GetHashCode ();
	}
}