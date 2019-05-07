using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that simulates penalty forces on box-shaped objects.
/// </summary>
/// <remarks>
/// This script automatically adds a BoxCollider component.
/// </remarks>
[RequireComponent (typeof (BoxCollider))]
public class BoxPenalty : Penalty {

	/// <summary>
	/// Class that stores information about one of the BoxCollider faces.
	/// </summary>
	private class PenaltyFace {

		/// <summary>
		/// A point in world coordinates that belongs to the face.
		/// </summary>
		public Vector3 point;

		/// <summary>
		/// The normal vector of this face. This vector is normalized.
		/// </summary>
		public Vector3 normal;

		/// <summary>
		/// Initializes a PenaltyFace instance.
		/// </summary>
		/// <param name="point">
		/// A point of the face represented in world coordinates.
		/// </param>
		/// <param name="normal">
		/// The face normal. The instance stores this vector normalized.
		/// </param>
		public PenaltyFace (Vector3 point, Vector3 normal) {
			this.point = point;
			this.normal = normal.normalized;
		}
	}

	/// <summary>
	/// List that contains each of the BoxCollider faces.
	/// </summary>
	private List<PenaltyFace> boxFaces;

	/// <summary>
	/// Array of vectors used for initializing the boxFaces list.
	/// </summary>
	/// <seealso cref="boxFaces"/>
	private readonly Vector3[] DIRECTIONS = {
		Vector3.up,
		Vector3.down,
		Vector3.right,
		Vector3.left,
		Vector3.forward,
		Vector3.back
	};

	/// <summary>
	/// On game start, calls the parent's Awake() method and initializes boxFaces list.
	/// </summary>
	protected new void Awake () {
		base.Awake ();
		InitializeFaces ();
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
    public override Matrix4x4 CalculatePenaltyMatrix(Node node)
    {
        PenaltyFace f = FindClosestFace(node);

        return BuildMatrix(new Vector4(f.normal.x, f.normal.y, f.normal.z, 0.0f));
    }

    /// <summary>
    /// Initializes the boxFaces list.
    /// </summary>
    /// <seealso cref="boxFaces"/>
    private void InitializeFaces () {
		boxFaces = new List<PenaltyFace> (6);
		Bounds boxBounds = GetComponent<BoxCollider> ().bounds;

		foreach (Vector3 d in DIRECTIONS) {
			Vector3 direction = transform.InverseTransformVector (d).normalized;
			Vector3 offset = new Vector3 (
				boxBounds.size.x * direction.x,
				boxBounds.size.y * direction.y,
				boxBounds.size.z * direction.z);
			Vector3 origin = boxBounds.center + offset;

			RaycastHit hit;
			Physics.Raycast (origin, -direction, out hit, PENALTY_LAYER);

			boxFaces.Add (new PenaltyFace (hit.point, hit.normal));
		}
	}

	/// <summary>
	/// Finds the closest face to the given node of the box.
	/// </summary>
	/// <param name="node">
	/// A node
	/// </param>
	/// <returns>
	/// A reference to the mentioned face.
	/// </returns>
	private PenaltyFace FindClosestFace (Node node) {
		PenaltyFace closestFace = null;
		float minDistance = float.PositiveInfinity;

		foreach (PenaltyFace t in boxFaces) {
			float distance = Vector3.Dot (t.normal, t.point - node.pos);
			if (distance < minDistance) {
				minDistance = distance;
				closestFace = t;
			}
		}

		return closestFace;
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
		return GetComponent<BoxCollider> ().bounds.Contains (worldPos);
	}

}