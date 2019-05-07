using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores the vertices that make up an edge plus the other vertex of the triangle that does not belong to the edge.
/// </summary>
public class Edge<T> {

    public T v1;
    public T v2;
    public T other;

    /// <summary>
    /// Initializes an Edge instance.
    /// </summary>
    /// <param name="v1">
    /// One of the vertices of the edge.</param>
    /// <param name="v2">
    /// The other vertex of the edge.</param>
    /// <param name="other">
    /// The vertex that with those of the edge make up a triangle.</param>
    public Edge (T v1, T v2, T other) {
        this.v1 = v1;
        this.v2 = v2;
        this.other = other;
    }

    /// <summary>
    /// Checks if two edges are equal.
    /// </summary>
    /// <remarks>
    /// Two edges are equal if the tuples of their vertices are identical.
    /// </remarks>
    /// <param name="obj">
    /// The edge to compare.
    /// </param>
    /// <returns>
    /// True if they fulfill the mentioned condition. False otherwise.
    /// </returns>
    public override bool Equals (object obj) {
        var otherEdge = obj as Edge<T>;
        if (otherEdge == null) {
            return false;
        }

        return v1.Equals(otherEdge.v1) && v2.Equals(otherEdge.v2) ||
            v1.Equals(otherEdge.v2) && v2.Equals(otherEdge.v1);
    }

    /// <summary>
    /// A hash code for the current edge.
    /// </summary>
    /// <returns>
    /// The edge's hash code.
    /// </returns>
    public override int GetHashCode () {
        return 13 * v1.GetHashCode () * v2.GetHashCode ();
    }

    /// <summary>
    /// Returns a string that represents the edge.
    /// </summary>
    /// <returns>
    /// Said string.
    /// </returns>
    public override string ToString () {
        return "{ " + v1 + ", " + v2 + " } + " + other;
    }
}