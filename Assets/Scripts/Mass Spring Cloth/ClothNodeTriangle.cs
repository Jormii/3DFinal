using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that defines a triangle of the MassSpringCloth mesh.
/// </summary>
public class ClothNodeTriangle : NodeTriangle {

    /// <summary>
    /// Dictionary that stores a NodeTriangle reference as key that maps the angle the normals of this triangle and the key's make.
    /// </summary>
    public Dictionary<NodeTriangle, float> theta0;
    public List<Edge<Node>> edges;

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
    public ClothNodeTriangle (Node n1, Node n2, Node n3, ClothSimulationParameters simParams):
        base (n1, n2, n3, simParams) {
            this.normal = CalculateNormal ();   // ¿Constructor padre?
            this.theta0 = new Dictionary<NodeTriangle, float> ();
            this.edges = new List<Edge<Node>> {
                new Edge<Node> (n1, n2, n3),
                new Edge<Node> (n1, n3, n2),
                new Edge<Node> (n2, n3, n1)
            };
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
        Vector3 n1Normal = ((ClothNode)n1).normal;
        Vector3 n2Normal = ((ClothNode)n2).normal;
        Vector3 n3Normal = ((ClothNode)n3).normal;
        return (n1Normal + n2Normal + n3Normal).normalized;
    }

    /// <summary>
    /// Adds a neighbour triangle to theta0 dictionary and calculates the angle their normals make.
    /// </summary>
    /// <param name="t">
    /// A neighbour triangle.
    /// </param>
    public void AddNeighbour (NodeTriangle t) {
        float angle = Vector3.Angle (normal, t.normal);
        theta0.Add (t, angle);
    }

    /// <summary>
    /// Calculates the forces affecting this triangle and applies them uniformly to its three nodes.
    /// </summary>
    public new void ComputeForce () {
        // Flexion force. If simParams.useFlexionSprings is true, dictionary will be empty.
        Vector3 force = Vector3.zero;
        float flexionSpringStiffness = ((ClothSimulationParameters)simParams).flexionSpringStiffness;
        foreach (KeyValuePair<NodeTriangle, float> entry in theta0) {
            NodeTriangle t = entry.Key;
            t.normal = t.CalculateNormal ();

            float _theta0 = theta0[t];
            float theta = Vector3.Angle (normal, t.normal);
            float energy = 0.5f * flexionSpringStiffness * (float) System.Math.Pow (theta - _theta0, 2);

            Vector3 flexionForce = -CalculateHessian (t) * energy;
            force += flexionForce;
        }

        force = force / 3.0f;
        n1.force += force;
        n2.force += force;
        n3.force += force;

        base.ComputeForce ();
    }

    private Vector3 CalculateHessian (NodeTriangle t) {
        Edge<Node> commonEdge = FindCommonEdge ((ClothNodeTriangle)t);
        if (commonEdge == null) {
            return Vector3.zero;
        }

        Vector3 x0 = edges.Find (e => e.Equals (commonEdge)).other.pos;
        Vector3 x1 = commonEdge.v1.pos;
        Vector3 x2 = commonEdge.v2.pos;
        Vector3 x3 = commonEdge.other.pos;
        Vector3 x1x2Vec = x2 - x1;

        float alpha1 = Vector3.Angle (x1x2Vec, x0 - x1);
        float alpha2 = Vector3.Angle (-x1x2Vec, x0 - x2);
        float alpha1Prime = Vector3.Angle (x1x2Vec, x3 - x1);
        float alpha2Prime = Vector3.Angle (-x1x2Vec, x3 - x2);

        float h0 = CalculateHeight (this, x1, x2);
        float h1 = CalculateHeight (this, x0, x2);
        float h2 = CalculateHeight (this, x0, x1);

        float h1Prime = CalculateHeight (t, x2, x3);
        float h2Prime = CalculateHeight (t, x1, x3);

        Vector3 x0Hess = -1.0f / h0 * normal;
        Vector3 x1Hess = Mathf.Cos (alpha2) / h1 * normal + Mathf.Cos (alpha2Prime) / h1Prime * t.normal;
        Vector3 x2Hess = Mathf.Cos (alpha1) / h2 * normal + Mathf.Cos (alpha1Prime) / h2Prime * t.normal;
        Vector3 x3Hess = -1.0f / h0 * t.normal;

        return x0Hess + x1Hess + x2Hess + x3Hess;
    }

    private float CalculateHeight (NodeTriangle t, Vector3 p1, Vector3 p2) {
        float b = Vector3.Distance (p1, p2);
        return (2.0f * t.area) / b;
    }

    private Edge<Node> FindCommonEdge (ClothNodeTriangle t) {
        foreach (Edge<Node> e in t.edges) {
            if (edges.Contains (e)) {
                return e;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns a string that represents the node triangle.
    /// </summary>
    /// <returns>
    /// Said string.
    /// </returns>
    public override string ToString () {
        string str = "Neighbour triangles:\n";
        foreach (KeyValuePair<NodeTriangle, float> entry in theta0) {
            str += "Theta 0: " + entry.Value + "\n\t" + entry.Key.n1 + "\n\t" + entry.Key.n2 + "\n\t" + entry.Key.n3 + "\n";
        }
        return "Normal: " + normal + ", Area: " + area + ", Nodes: {\n\t" + n1 + "\n\t" + n2 + "\n\t" + n3 + "\n}\n" + str;
    }

}