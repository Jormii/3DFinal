using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class that defines penalty force behaviour.
/// </summary>
public abstract class Penalty : MonoBehaviour {

    /// <summary>
    /// Penalty constant.
    /// </summary>
    public float k = 1000000.0f;

    /// <summary>
    /// Every penalty object will be located in a particular layer in order to avoid interferences with other colliders.
    /// </summary>
    public const int PENALTY_LAYER = 9;

    /// <summary>
    /// On game start, gameobject's tag and layer are set accordingly.
    /// </summary>
    protected void Awake () {
        gameObject.tag = "Penalty";
        gameObject.layer = PENALTY_LAYER;
    }

    /// <summary>
    /// Calculates the dF/dx matrix.
    /// </summary>
    /// <param name="n">
    /// The normal of the surface penetrated.
    /// </param>
    /// <returns>
    /// A 4x4 matrix containing the 3x3 dF/dx matrix.
    /// </returns>
    /// <remarks>
    /// The returned matrix is 4x4 because Unity does not support 3x3 matrix.
    /// </remarks>
    protected Matrix4x4 BuildMatrix (Vector4 n) {
        Vector4 firstsRow = -k * new Vector4 (n.x * n.x, n.x * n.y, n.x * n.z, n.x * n.w);
        Vector4 secondRow = -k * new Vector4 (n.y * n.x, n.y * n.y, n.y * n.z, n.y * n.w);
        Vector4 thirdRow = -k * new Vector4 (n.z * n.x, n.z * n.y, n.z * n.z, n.z * n.w);
        Vector4 fourthRow = -k * new Vector4 (n.w * n.x, n.w * n.y, n.w * n.z, n.w * n.w);

        Matrix4x4 matrix = new Matrix4x4 ();
        matrix.SetRow (0, firstsRow);
        matrix.SetRow (1, secondRow);
        matrix.SetRow (2, thirdRow);
        matrix.SetRow (3, fourthRow);

        return matrix;
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
    public abstract Matrix4x4 CalculatePenaltyMatrix (Node node);

    /// <summary>
    /// Checks if a point is contained in the penalty object.
    /// </summary>
    /// <param name="worldPos">
    /// The point to check, in world coordinates.
    /// </param>
    /// <returns>
    /// Returns true if the penalty object contains the point. False otherwise.
    /// </returns>
    public abstract bool Contains (Vector3 worldPos);

}