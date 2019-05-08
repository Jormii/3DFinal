using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores the information of one Tetrahedron.
/// </summary>
public class Tetrahedron {

	/// <summary>
	/// Class that stores the nodes weights of the Tetrahedron.
	/// </summary>	
	public class Weights {

		public List<int> ids;
		public float w1;
		public float w2;
		public float w3;
		public float w4;

		/// <summary>
		/// Initializes a Tetrahedron instance.
		/// </summary>
		/// <param name="id">
		/// Mesh's vertex's id contained by this Tetrahedron.
		/// </param>
		/// <param name="w1">
		/// Weight of the node "n1" of the Tetrahedron affecting the vertex id.
		/// </param>
		/// <param name="w2">
		/// Weight of the node "n2" of the Tetrahedron affecting the vertex id.
		/// </param>
		/// <param name="w3">
		/// Weight of the node "n3" of the Tetrahedron affecting the vertex id.
		/// </param>
		/// <param name="w4">
		/// Weight of the node "n4" of the Tetrahedron affecting the vertex id.
		/// </param>
		public Weights (int id, float w1, float w2, float w3, float w4) {
			this.ids = new List<int> () {
				id
			};
			this.w1 = w1;
			this.w2 = w2;
			this.w3 = w3;
			this.w4 = w4;
		}

		/// <summary>
		/// Adds a vertex id to this weight's list of vertices.
		/// </summary>
		/// <param name="id">
		/// The id of the vertex.
		/// </param>
		public void AddId (int id) {
			ids.Add (id);
		}

		/// <summary>
		/// Returns a string that represents the tetrahedron.
		/// </summary>
		/// <returns>
		/// Said string.
		/// </returns>
		public override string ToString () {
			string idsString = "[";
			foreach (int id in ids) {
				idsString += " " + id;
			}
			idsString += " ]";

			return string.Format ("W1: {0}; W2: {1}; W3: {2}; W4: {3}; Vertices ids: {4}",
				w1, w2, w3, w4, idsString);
		}
	}

	public RigidBodyNode n1;
	public RigidBodyNode n2;
	public RigidBodyNode n3;
	public RigidBodyNode n4;
	public float mass;
	public float volume;
	public Dictionary<Vector3, Weights> meshVertices;
	public RigidBodySimulationParameters simParams;

	/// <summary>
	/// Initializes a Tetrahedron instance.
	/// </summary>
	/// <param name="n1">
	/// One of the nodes that make up the tetrahedron.
	/// </param>
	/// <param name="n2">
	/// The second of the nodes that make up the tetrahedron.
	/// </param>
	/// <param name="n3">
	/// The third of the nodes that make up the tetrahedron.
	/// </param>
	/// <param name="n4">
	/// The last of the nodes that make up the tetrahedron.
	/// </param>
	/// <param name="simParams">
	/// A reference to the instance that holds the simulation parameters.
	/// </param>
	public Tetrahedron (RigidBodyNode n1, RigidBodyNode n2, RigidBodyNode n3, RigidBodyNode n4, RigidBodySimulationParameters simParams) {
		this.n1 = n1;
		this.n2 = n2;
		this.n3 = n3;
		this.n4 = n4;
		this.meshVertices = new Dictionary<Vector3, Weights> ();
		this.simParams = simParams;
		Initialize ();
	}

	/// <summary>
	/// Checks if a vertex is contained in this tetrahedron.
	/// </summary>
	/// <param name="v">
	/// The vertex to be checked, in world coordinates.
	/// </param>
	/// <param name="id">
	/// The id of the vertex.
	/// </param>
	/// <returns>
	/// True if the tetrahedron contains the vertex. False otherwise.
	/// </returns>
	/// <remarks>
	/// The tetrahedron updates its variables in case the vertex is contained by it.
	/// </remarks>
	public bool Contains (Vector3 v, int id) {
		// In case a vertex has multiples ids associated.
		if (meshVertices.ContainsKey (v)) {
			meshVertices[v].AddId (id);
			return true;
		}

		float w1 = CalculateTetrahedronVolume (v, n2.pos, n3.pos, n4.pos) / volume;
		float w2 = CalculateTetrahedronVolume (n1.pos, v, n3.pos, n4.pos) / volume;
		float w3 = CalculateTetrahedronVolume (n1.pos, n2.pos, v, n4.pos) / volume;
		float w4 = CalculateTetrahedronVolume (n1.pos, n2.pos, n3.pos, v) / volume;
		float w = w1 + w2 + w3 + w4;

		bool contains = NearlyEqual (w, 1.0f, 0.0001f);
		if (contains) {
			meshVertices.Add (v, new Weights (id, w1, w2, w3, w4));
		}

		return contains;
	}

	/// <summary>
	/// Calculates if two float values are approximately equal.
	/// </summary>
	/// <param name="a">
	/// One of the values to be checked.
	/// </param>
	/// <param name="b">
	/// The other float value.
	/// </param>
	/// <param name="eplison">
	/// The error margin when compairing the values.
	/// </param>
	/// <returns>
	/// True if the values are nearly equal. False otherwise.
	/// </returns>
	/// <remarks>
	/// Source: https://floating-point-gui.de/errors/comparison/
	/// </remarks>
	public static bool NearlyEqual (float a, float b, float epsilon) {
		float absA = Mathf.Abs (a);
		float absB = Mathf.Abs (b);
		float diff = Mathf.Abs (a - b);

		if (a == b) { // shortcut, handles infinities
			return true;
		} else if (a == 0 || b == 0 || diff < float.MinValue) {
			// a or b is zero or both are extremely close to it
			// relative error is less meaningful here
			return diff < (epsilon * float.MinValue);
		} else { // use relative error
			return diff / (absA + absB) < epsilon;
		}
	}

	/// <summary>
	/// Initializes the values of the tetrahedron and updates its nodes.
	/// </summary>
	private void Initialize () {
		volume = CalculateTetrahedronVolume (n1.pos, n2.pos, n3.pos, n4.pos);
		mass = simParams.density * volume;

		float nodeMass = mass / 4.0f;
		n1.mass += nodeMass;
		n2.mass += nodeMass;
		n3.mass += nodeMass;
		n4.mass += nodeMass;
	}

	/// <summary>
	/// Interpolates the position of one vertex contained in this node.
	/// </summary>
	/// <param name="w">
	/// The weights instance associated with the vertex.
	/// </param>
	/// <returns>
	/// The interpolated position of the vertex, in global coordinates.
	/// </returns>
	public Vector3 InterpolatePosition (Weights w) {
		return n1.pos * w.w1 + n2.pos * w.w2 + n3.pos * w.w3 + n4.pos * w.w4;
	}

	/// <summary>
	/// Calculates the volume of a tetrahedron.
	/// </summary>
	/// <param name="v1">
	/// The position of one of the vertices that make up the tetrahedron, in global coordinates.
	/// </param>
	/// <param name="v2">
	/// The position of the second of the vertices that make up the tetrahedron, in global coordinates.
	/// </param>
	/// <param name="v3">
	/// The position of the third of the vertices that make up the tetrahedron, in global coordinates.
	/// </param>
	/// <param name="v4">
	/// The position of the last of the vertices that make up the tetrahedron, in global coordinates.
	/// </param>
	/// <returns>
	/// The volume of the tetrahedron.
	/// </returns>
	public static float CalculateTetrahedronVolume (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		Vector3 p1_p0 = v1 - v4;
		Vector3 p2_p0 = v2 - v4;
		Vector3 p3_p0 = v3 - v4;

		return Mathf.Abs (Vector3.Dot (p1_p0, Vector3.Cross (p2_p0, p3_p0)) / 6.0f);
	}

	/// <summary>
	/// Returns a string that represents the tetrahedron.
	/// </summary>
	/// <returns>
	/// Said string.
	/// </returns>
	public override string ToString () {
		return string.Format ("Tetrahedron with Mass = {0}, Volume = {1}, Nodes =\n\t{2},\n\t{3},\n\t{4},\n\t{5}\n",
			mass, volume, n1, n2, n3, n4);
	}
}