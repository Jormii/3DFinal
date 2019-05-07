using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that simulates penalty forces on sphere-shaped objects.
/// </summary>
/// <remarks>
/// This script automatically adds a SphereCollider component.
/// </remarks>
[RequireComponent (typeof (SphereCollider))]
public class SpherePenalty : Penalty {

    /// <summary>
    /// The center of the SphereCollider, in world coordinates.
    /// </summary>
    private Vector3 center;

    /// <summary>
    /// The radius of the SphereCollider.
    /// </summary>
    private float radius;

    /// <summary>
    /// On game start, calls parent's Awake() method and initializes script variables.
    /// </summary>
    protected new void Awake () {
        base.Awake ();
        center = GetComponent<SphereCollider> ().bounds.center;
        radius = 0.5f * GetComponent<SphereCollider> ().bounds.size.x;
    }

    /// <summary>
    /// Calculates the penalty force that should be applied to the given node in order to resolve the colission.
    /// </summary>
    /// <param name="node">
    /// The node collisioning with the object.
    /// </param>
    /// <returns>
    /// A 4x4Matrix containing the 3x3 dF/dx matrix.
    /// </returns>
    public override Matrix4x4 CalculatePenaltyMatrix (Node node) {
        Vector3 u = node.pos - center;
        Vector3 normal = u.normalized;

        return BuildMatrix (new Vector4 (normal.x, normal.y, normal.z, 0.0f));
    }

    /// <summary>
    /// Checks if a point is contained in the penalty object.
    /// </summary>
    /// <param name="worldPos">
    /// The point to check, in world coordinates.
    /// </param>
    /// <returns>
    /// Returns true if the penalty object contains the point. False otherwise.
    /// </returns>
    public override bool Contains (Vector3 worldPos) {
        return Vector3.Distance (worldPos, center) <= radius;
    }
}