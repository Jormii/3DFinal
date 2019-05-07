using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NodeTriangle {

	public Node n1;
    public Node n2;
    public Node n3;
    public Vector3 normal;
    public float area;
	public SimulationParameters simParams;

	/// <summary>
    /// Initializes a NodeTriangle instance.
    /// </summary>
    /// <param name="n1">
    /// One of the nodes that make up the triangle.
    /// </param>
    /// <param name="n2">
    /// Another of the nodes that make up the triangle.
    /// </param>
    /// <param name="n3">
    /// The last of the nodes that make up the triangle.
    /// </param>
    /// <param name="simParams">
    /// A reference to an instance that holds the parameters of the simulation.
    /// </param>
    public NodeTriangle (Node n1, Node n2, Node n3, SimulationParameters simParams) {
        this.n1 = n1;
        this.n2 = n2;
        this.n3 = n3;
        this.area = CalculateArea ();
        this.simParams = simParams;
    }

	/// <summary>
    /// Calculates the area of the triangle formed by the three nodes.
    /// </summary>
    /// <returns>
    /// The area of the triangle.
    /// </returns>
    public float CalculateArea () {
        Vector3 v1v2Vec = n2.pos - n1.pos;
        Vector3 v1v3Vec = n3.pos - n1.pos;

        return 0.5f * Vector3.Cross (v1v2Vec, v1v3Vec).magnitude;
    }

    /// <summary>
    /// Calculates the normal of this triangle.
    /// </summary>
    /// <returns>
    /// The normal vector of this triangle.
    /// </returns>
	public abstract Vector3 CalculateNormal();

	/// <summary>
    /// Calculates the forces affecting this triangle and applies them uniformly to its three nodes.
    /// </summary>
    public void ComputeForce () {
        /// Wind force
        Vector3 triVel = (n1.vel + n2.vel + n3.vel) / 3.0f;
        normal = CalculateNormal();
		area = CalculateArea();

        Vector3 force = simParams.windDamping * area * Vector3.Dot (normal, simParams.windVector - triVel) * normal;

        force = force / 3.0f;
        n1.force += force;
        n2.force += force;
        n3.force += force;
    }
}
