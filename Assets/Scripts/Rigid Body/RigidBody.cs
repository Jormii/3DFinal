using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class responsible of rigidbody simulation.
/// </summary>
/// <remarks>
/// If no .mesh file is provided, the object is destroyed.
/// </remarks>
public class RigidBody : MonoBehaviour {

	public TextAsset dotMeshFile;
	public RigidBodySimulationParameters simParams = new RigidBodySimulationParameters ();

	private bool initialized = false;
	private Mesh mesh;
	private Dictionary<Vector3, RigidBodyNode> nodes;
	private Dictionary<RigidBodySpring, RigidBodySpring> springs;
	private HashSet<RigidBodyNodeTriangle> triangles;
	private List<Tetrahedron> tetrahedrons;
	private List<GameObject> fixers;
	private List<GameObject> penaltyEntities;

	/// <summary>
	/// On game start, initialize the data structures.
	/// </summary>
	void Awake () {
		if (dotMeshFile == null) {
			Debug.LogError ("[RigidBody.cs]: No .mesh file found on gameobject \"" + gameObject.name + "\". Destroying gameobject.");
			GameObject.Destroy (gameObject);
		}

		mesh = GetComponent<MeshFilter> ().mesh;
		mesh.MarkDynamic ();

		fixers = new List<GameObject> ();
		penaltyEntities = new List<GameObject> ();
	}

	/// <summary>
	/// On script initialization, gets fixers and penalty objects and creates the virtual mesh.
	/// </summary>
	void Start () {
		for (int c = 0; c < transform.childCount; ++c) {
			Transform t = transform.GetChild (c);
			switch (t.tag) {
				case "Fixer":
					fixers.Add (t.gameObject);
					break;
				case "Penalty":
					penaltyEntities.Add (t.gameObject);
					break;
				default:
					break;
			}
		}

		Initialize ();

		string debug = String.Format ("{0} [RigidBody.cs] => Nodes: {1}; Springs: {2}; Triangles: {3}; Tetrahedrons: {4}",
			gameObject.name, nodes.Count, springs.Count, triangles.Count, tetrahedrons.Count);
		Debug.LogWarning (debug);

		if (simParams.debugNodes) {
			DebugNodes ();
		}

		if (simParams.debugSprings) {
			DebugSprings ();
		}

		if (simParams.debugTriangles) {
			DebugTriangles ();
		}

		if (simParams.debugTetrahedrons) {
			DebugTetrahedrons ();
		}
	}

	/// <summary>
	/// Draws the tetrahedron mesh.
	/// </summary>
	void OnDrawGizmos () {
		if (!initialized) {
			return;
		}

		Gizmos.color = Color.blue;
		foreach (Spring spring in springs.Keys) {
			Gizmos.DrawLine (spring.nodeA.pos, spring.nodeB.pos);
		}

		Gizmos.color = Color.green;
		foreach (Node n in nodes.Values) {
			Gizmos.DrawSphere (n.pos, 0.05f);
		}

	}

	/// <summary>
	/// Each frame, mesh is stored according to the changes in the tetrahedron mesh.
	/// </summary>
	void Update () {
		if (simParams.pauseSimulation || !initialized) {
			return;
		}

		Vector3[] meshNewPositions = mesh.vertices;
		foreach (Tetrahedron t in tetrahedrons) {
			foreach (KeyValuePair<Vector3, Tetrahedron.Weights> entry in t.meshVertices) {
				Vector3 newPosition = t.InterpolatePosition (entry.Value);
				Vector3 localPosition = transform.InverseTransformPoint (newPosition);
				foreach (int id in entry.Value.ids) {
					meshNewPositions[id] = localPosition;
				}
			}
		}

		mesh.vertices = meshNewPositions;
	}

	/// <summary>
	/// Each fixed update, the script interpolates nodes' positions and velocities.
	/// </summary>
	void FixedUpdate () {
		if (simParams.pauseSimulation || !initialized) {
			return;
		}

		for (int i = 0; i < simParams.subSteps; ++i) {
			switch (simParams.integrationMethod) {
				case SimulationParameters.IntegrationMethod.ExplicitEuler:
					StepExplicit ();
					break;
				case SimulationParameters.IntegrationMethod.SymplecticEuler:
					StepSymplectic ();
					break;
			}
		}
	}

	/// <summary>
	/// Performs position and velocity interpolation using Explicit Euler.
	/// </summary>
	private void StepExplicit () {
		ComputeForces ();

		foreach (KeyValuePair<Vector3, RigidBodyNode> entry in nodes) {
			RigidBodyNode node = entry.Value;
			if (!node.isFixed) {
				node.pos += simParams.timeStep * node.vel;
				node.vel = CalculateImplicitVelocity (node);
			}
		}
	}

	/// <summary>
	/// Performs position and velocity interpolation using Symplectic Euler.
	/// </summary>
	private void StepSymplectic () {
		ComputeForces ();

		foreach (KeyValuePair<Vector3, RigidBodyNode> entry in nodes) {
			RigidBodyNode node = entry.Value;
			if (!node.isFixed) {
				node.vel = CalculateImplicitVelocity (node);
				node.pos += simParams.timeStep * node.vel;
			}
		}
	}

	private Vector3 CalculateImplicitVelocity (RigidBodyNode n) {
		Vector3 v = n.vel + (simParams.timeStep / n.mass) * n.force;
		Matrix4x4 dF_dx = n.dF_dx;

		if (dF_dx.Equals (Matrix4x4.zero)) {
			return v;
		}

		Matrix4x4 I = Matrix4x4.identity;
		float H = Mathf.Pow (simParams.timeStep, 2.0f) / n.mass;

		dF_dx.SetRow (0, dF_dx.GetRow (0) * H);
		dF_dx.SetRow (1, dF_dx.GetRow (1) * H);
		dF_dx.SetRow (2, dF_dx.GetRow (2) * H);
		dF_dx.SetRow (3, dF_dx.GetRow (3) * H);

		Matrix4x4 I_minus_P = new Matrix4x4 ();
		I_minus_P.SetRow (0, I.GetRow (0) - dF_dx.GetRow (0));
		I_minus_P.SetRow (1, I.GetRow (1) - dF_dx.GetRow (1));
		I_minus_P.SetRow (2, I.GetRow (2) - dF_dx.GetRow (2));
		I_minus_P.SetRow (3, I.GetRow (3) - dF_dx.GetRow (3));

		I_minus_P = I_minus_P.inverse;

		return I_minus_P.MultiplyVector (v);
	}

	/// <summary>
	/// Calculates the forces affecting the tetrahedrons mesh.
	/// </summary>
	private void ComputeForces () {
		foreach (KeyValuePair<Vector3, RigidBodyNode> entry in nodes) {
			Node n = entry.Value;
			if (!n.isFixed) {
				n.Reset ();
				n.ComputeForce ();

				foreach (GameObject penaltyEntity in penaltyEntities) {
					Penalty penaltyComponent = penaltyEntity.GetComponent<Penalty> ();
					if (penaltyComponent.Contains (n.pos)) {
						n.ApplyPenaltyForce (penaltyComponent);
					}
				}
			}
		}

		foreach (Spring s in springs.Values) {
			s.ComputeForce ();
		}

		foreach (NodeTriangle t in triangles) {
			t.ComputeForce ();
		}
	}

	/// <summary>
	/// Initializes the tetrahedron bounding box and the object's mesh.
	/// </summary>
	private void Initialize () {
		InitializeBoundingBox ();
		InitializeMesh ();
		initialized = true;
	}

	/// <summary>
	/// Initializes all the data structures necessaries to simulate the tetrahedron bounding box.
	/// </summary>
	private void InitializeBoundingBox () {
		Dictionary<string, List<DotMeshField>> fields = DotMeshParser.Parse (AssetDatabase.GetAssetPath (dotMeshFile));

		foreach (KeyValuePair<string, List<DotMeshField>> entry in fields) {
			string fieldType = entry.Key;

			switch (fieldType) {
				case DotMeshParser.verticesField:
					InitializeVertices (entry.Value);
					break;
				case DotMeshParser.trianglesField:
					InitializeTriangles (entry.Value, fields[DotMeshParser.verticesField]);
					break;
				case DotMeshParser.tetrahedronField:
					InitializeTetrahedrons (entry.Value, fields[DotMeshParser.verticesField]);
					break;
				case DotMeshParser.cornersField:
					// Does nothing
					InitializeCorners (entry.Value);
					break;
				case DotMeshParser.edgesField:
					// Does nothing
					InitializeEdges (entry.Value);
					break;
				default:
					break;
			}
		}
	}

	/// <summary>
	/// Initializes the nodes of the tetrahedron mesh.
	/// </summary>
	/// <param name="tetraVertices">
	/// A list containing the vertices of the tretahedron mesh.
	/// </param>
	private void InitializeVertices (List<DotMeshField> tetraVertices) {
		nodes = new Dictionary<Vector3, RigidBodyNode> (tetraVertices.Count);
		for (int i = 0; i < tetraVertices.Count; ++i) {
			DotMeshVertex v = (DotMeshVertex) tetraVertices[i];

			int id = i;
			Vector3 worldPos = new Vector3 (v.v1Pos, v.v2Pos, v.v3Pos);
			bool isFixed = CheckFixers (worldPos);
			nodes.Add (worldPos, new RigidBodyNode (id, worldPos, isFixed, simParams));
		}
	}

	/// <summary>
	/// Initializes the triangles of the tetrahedron mesh.
	/// </summary>
	/// <param name="tetraTriangles">
	/// A list containing the triangles of the tetrahedron mesh.
	/// </param>
	/// <param name="tetraVertices">
	/// A list containing the vertices of the tretahedron mesh.
	/// </param>
	private void InitializeTriangles (List<DotMeshField> tetraTriangles, List<DotMeshField> tetraVertices) {
		triangles = new HashSet<RigidBodyNodeTriangle> ();

		for (int i = 0; i < tetraTriangles.Count; ++i) {
			DotMeshTriangle t = (DotMeshTriangle) tetraTriangles[i];

			// Inner triangles hold a reference value of 0
			if (t.reference == 1) {
				DotMeshVertex v1 = (DotMeshVertex) tetraVertices[t.v1 - 1];
				DotMeshVertex v2 = (DotMeshVertex) tetraVertices[t.v2 - 1];
				DotMeshVertex v3 = (DotMeshVertex) tetraVertices[t.v3 - 1];

				RigidBodyNode n1 = nodes[new Vector3 (v1.v1Pos, v1.v2Pos, v1.v3Pos)];
				RigidBodyNode n2 = nodes[new Vector3 (v2.v1Pos, v2.v2Pos, v2.v3Pos)];
				RigidBodyNode n3 = nodes[new Vector3 (v3.v1Pos, v3.v2Pos, v3.v3Pos)];

				triangles.Add (new RigidBodyNodeTriangle (n1, n2, n3, simParams));
			}
		}
	}

	/// <summary>
	/// Initializes the tetrahedrons of the tetrahedron mesh.
	/// </summary>
	/// <param name="tetraTetrahedrons">
	/// A list containing the tetrahedrons of the tetrahedron mesh.
	/// </param>
	/// <param name="tetraVertices">
	/// A list containing the vertices of the tretahedron mesh.
	/// </param>
	private void InitializeTetrahedrons (List<DotMeshField> tetraTetrahedrons, List<DotMeshField> tetraVertices) {
		springs = new Dictionary<RigidBodySpring, RigidBodySpring> (tetraTetrahedrons.Count >> 1);
		tetrahedrons = new List<Tetrahedron> (tetraTetrahedrons.Count);

		for (int i = 0; i < tetraTetrahedrons.Count; ++i) {
			DotMeshTetrahedron t = (DotMeshTetrahedron) tetraTetrahedrons[i];

			DotMeshVertex v1 = (DotMeshVertex) tetraVertices[t.v1 - 1];
			DotMeshVertex v2 = (DotMeshVertex) tetraVertices[t.v2 - 1];
			DotMeshVertex v3 = (DotMeshVertex) tetraVertices[t.v3 - 1];
			DotMeshVertex v4 = (DotMeshVertex) tetraVertices[t.v4 - 1];

			RigidBodyNode n1 = nodes[new Vector3 (v1.v1Pos, v1.v2Pos, v1.v3Pos)];
			RigidBodyNode n2 = nodes[new Vector3 (v2.v1Pos, v2.v2Pos, v2.v3Pos)];
			RigidBodyNode n3 = nodes[new Vector3 (v3.v1Pos, v3.v2Pos, v3.v3Pos)];
			RigidBodyNode n4 = nodes[new Vector3 (v4.v1Pos, v4.v2Pos, v4.v3Pos)];

			Tetrahedron tetrahedron = new Tetrahedron (n1, n2, n3, n4, simParams);
			tetrahedrons.Add (tetrahedron);

			float springVolume = tetrahedron.volume / 6.0f;
			RigidBodySpring[] tetrahedronSprings = new RigidBodySpring[] {
				new RigidBodySpring (n1, n2, springVolume, simParams),
					new RigidBodySpring (n1, n3, springVolume, simParams),
					new RigidBodySpring (n1, n4, springVolume, simParams),
					new RigidBodySpring (n2, n3, springVolume, simParams),
					new RigidBodySpring (n2, n4, springVolume, simParams),
					new RigidBodySpring (n3, n4, springVolume, simParams)
			};

			foreach (RigidBodySpring s in tetrahedronSprings) {
				if (springs.ContainsKey (s)) {
					springs[s].volume += springVolume;
				} else {
					springs.Add (s, s);
				}
			}
		}
	}

	/// <summary>
	/// Initializes the object's mesh.
	/// </summary>
	/// <remarks>
	/// This method finds which tetrahedrons of the bounding box contain the vertices of mesh.
	/// </remarks>
	private void InitializeMesh () {
		int[] vertsIds = mesh.triangles;
		Vector3[] vertsPos = mesh.vertices;

		foreach (int id in vertsIds) {
			Vector3 worldPos = transform.TransformPoint (vertsPos[id]);
			foreach (Tetrahedron t in tetrahedrons) {
				if (t.Contains (worldPos, id)) {
					break;
				}
			}
		}
	}

	/// <summary>
	/// Initializes the corners of the tetrahedron mesh.
	/// </summary>
	/// <param name="tetraCorners">
	/// A list containing the corners of the tetrahedron mesh.
	/// </param>
	/// <remarks>
	/// This method does nothing.
	/// </remarks>
	private void InitializeCorners (List<DotMeshField> tetraCorners) { }

	/// <summary>
	/// Initializes the edges of the tetrahedron mesh.
	/// </summary>
	/// <param name="tetraEdges">
	/// A list containing the edges of the tetrahedron mesh.
	/// </param>
	/// <remarks>
	/// This method does nothing.
	/// </remarks>
	private void InitializeEdges (List<DotMeshField> tetraEdges) { }

	/// <summary>
	/// Checks if a point is contained in any of the children that use the Fixer component.
	/// </summary>
	/// <param name="worldPosition">
	/// The point being checked, in world coordinates.
	/// </param>
	/// <returns>
	/// True if the point is contained in any of the objects. False otherwise.
	/// </returns>
	private bool CheckFixers (Vector3 worldPosition) {
		foreach (GameObject fixer in fixers) {
			Bounds fixerBounds = fixer.GetComponent<Collider> ().bounds;
			if (fixerBounds.Contains (worldPosition)) {
				return true;
			}
		}
		return false;
	}

	///
	/// Debug methods ahead.
	///

	/// <summary>
	/// Prints in Unity's console the cloth's nodes.
	/// </summary>
	private void DebugNodes () {
		foreach (KeyValuePair<Vector3, RigidBodyNode> entry in nodes) {
			Debug.Log (entry.Value);
		}
	}

	/// <summary>
	/// Prints in Unity's console the cloth's springs.
	/// </summary>
	private void DebugSprings () {
		foreach (RigidBodySpring s in springs.Values) {
			Debug.Log (s);
		}
	}

	/// <summary>
	/// Prints in Unity's console the cloth's triangles.
	/// </summary>
	private void DebugTriangles () {
		foreach (RigidBodyNodeTriangle t in triangles) {
			Debug.Log (t);
		}
	}

	/// <summary>
	/// Prints in Unity's console the cloth's triangles.
	/// </summary>
	private void DebugTetrahedrons () {
		foreach (Tetrahedron t in tetrahedrons) {
			Debug.Log (t);
			foreach (object o in t.meshVertices) {
				Debug.Log (o);
			}
		}
	}
}