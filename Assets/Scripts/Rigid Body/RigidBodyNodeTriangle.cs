using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that defines a triangle of the MassSpringCloth mesh.
/// </summary>
public class RigidBodyNodeTriangle : NodeTriangle {

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
    public RigidBodyNodeTriangle (Node n1, Node n2, Node n3, RigidBodySimulationParameters simParams):
        base (n1, n2, n3, simParams) {
            this.normal = CalculateNormal ();
        }

    /// <summary>
    /// Calculates the normal of this triangle.
    /// </summary>
    /// <returns>
    /// The normal vector of this triangle.
    /// </returns>
    /// <remarks>
    /// This method is designed to work with .mesh files.
    /// </remarks>
    public override Vector3 CalculateNormal () {
        Vector3 v1v2Vec = n2.pos - n1.pos;
        Vector3 v1v3Vec = n3.pos - n1.pos;

        return -Vector3.Cross (v1v2Vec, v1v3Vec).normalized;
    }

    /// <summary>
    /// Returns a string that represents the node triangle.
    /// </summary>
    /// <returns>
    /// Said string.
    /// </returns>
    public override string ToString () {
        return "Normal: " + normal + ", Area: " + area + ", Nodes: {\n\t" + n1 + "\n\t" + n2 + "\n\t" + n3 + "\n}";
    }

}