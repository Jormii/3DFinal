using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that stores the information of one triangle.
/// </summary>
public class Triangle {

    private int v1;
    private int v2;
    private int v3;

    /// <summary>
    /// A list with the edges that connect the vertices of the triangle.
    /// </summary>
    public List<Edge<int>> edges;

    /// <summary>
    /// Initializes a Triangle instance.
    /// </summary>
    /// <param name="v1">
    /// One of the vertices of the triangle.
    /// </param>
    /// <param name="v2">
    /// Another of the vertices of the triangle.
    /// </param>
    /// <param name="v3">
    /// The last vertex of the triangle.
    /// </param>
    public Triangle (int v1, int v2, int v3) {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;

        edges = new List<Edge<int>> (3) {
            new Edge<int> (v1, v2, v3),
            new Edge<int> (v1, v3, v2),
            new Edge<int> (v2, v3, v1)
        };
    }

    /// <summary>
    /// Returns a string that represents the triangle.
    /// </summary>
    public override string ToString () {
        return "{ " + v1 + ", " + v2 + ", " + v3 + " }";
    }

}