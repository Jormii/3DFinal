using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible of cloth simulation. Works on planes.
/// </summary>
public class MassSpringCloth : MonoBehaviour {

	public ClothSimulationParameters simParams = new ClothSimulationParameters ();

	private bool initialized = false;
	private Mesh mesh;
	private Dictionary<Vector3, Node> nodes;
	private HashSet<Spring> springs;
	private HashSet<NodeTriangle> triangles;
	private Dictionary<Edge<int>, int> edgesFlexionSprings;
	private Dictionary<Edge<int>, NodeTriangle> edgesFlexion;
	private List<GameObject> fixers;
	private List<GameObject> penaltyEntities;

	/// <summary>
	/// On game start, initializes the data structures.
	/// </summary>
	void Awake () {
		mesh = GetComponent<MeshFilter> ().mesh;
		mesh.MarkDynamic ();

		nodes = new Dictionary<Vector3, Node> (mesh.vertexCount);
		springs = new HashSet<Spring> ();
		triangles = new HashSet<NodeTriangle> ();
		edgesFlexionSprings = new Dictionary<Edge<int>, int> ();
		edgesFlexion = new Dictionary<Edge<int>, NodeTriangle> ();
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

		Debug.LogWarning (gameObject.name + " (MassSpringCloth) => Nodes: " + nodes.Count + " Springs: " + springs.Count +
			" Triangles: " + triangles.Count);

		if (simParams.debugNodes) {
			DebugNodes ();
		}

		if (simParams.debugSprings) {
			DebugSprings ();
		}

		if (simParams.debugTriangles) {
			DebugTriangles ();
		}
	}

	/// <summary>
	/// Each frame, plane mesh is updated with the information stored in the nodes.
	/// </summary>
	void Update () {
		if (simParams.pauseSimulation || !initialized) {
			return;
		}

		Vector3[] newPositions = mesh.vertices;
		foreach (KeyValuePair<Vector3, Node> entry in nodes) {
			Node n = entry.Value;
			Vector3 local = transform.InverseTransformPoint (n.pos);
			foreach (int id in n.nodeIds) {
				newPositions[id] = local;
			}
		}

		mesh.vertices = newPositions;
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
			mesh.RecalculateNormals ();
		}
	}

	/// <summary>
	/// Performs position and velocity interpolation using Explicit Euler.
	/// </summary>
	private void StepExplicit () {
		ComputeForces ();

		foreach (KeyValuePair<Vector3, Node> entry in nodes) {
			Node node = entry.Value;
			if (!node.isFixed) {
				node.pos += simParams.timeStep * node.vel;
				node.vel += CalculateImplicitVelocity (node);
				((ClothNode) node).normal = transform.TransformVector (mesh.normals[node.nodeIds[0]]);
			}
		}
	}

	/// <summary>
	/// Performs position and velocity interpolation using Symplectic Euler.
	/// </summary>
	private void StepSymplectic () {
		ComputeForces ();

		foreach (KeyValuePair<Vector3, Node> entry in nodes) {
			Node node = entry.Value;
			if (!node.isFixed) {
				node.vel = CalculateImplicitVelocity (node);
				node.pos += simParams.timeStep * node.vel;
				((ClothNode) node).normal = transform.TransformVector (mesh.normals[node.nodeIds[0]]);
			}
		}
	}

	private Vector3 CalculateImplicitVelocity (Node n) {
		float nodeMass = ((ClothSimulationParameters) simParams).nodeMass;
		Vector3 v = n.vel + (simParams.timeStep / nodeMass) * n.force;
		Matrix4x4 dF_dx = n.dF_dx;

		if (dF_dx.Equals (Matrix4x4.zero)) {
			return v;
		}

		Matrix4x4 I = Matrix4x4.identity;
		float H = Mathf.Pow (simParams.timeStep, 2.0f) / nodeMass;

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
	/// Calculates the forces affecting the cloth.
	/// </summary>
	private void ComputeForces () {
		foreach (KeyValuePair<Vector3, Node> entry in nodes) {
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

		foreach (Spring s in springs) {
			s.ComputeForce ();
		}

		foreach (NodeTriangle t in triangles) {
			t.ComputeForce ();
		}
	}

	/// <summary>
	/// Initializes all the data structures necessaries to represent and manipulate the cloth.
	/// </summary>
	private void Initialize () {
		int[] verticesIds = mesh.triangles;
		Vector3[] verticesPos = mesh.vertices;

		for (int i = 0; i < verticesIds.Length; i += 3) {
			// Inicializacion del triangulo
			int[] vertices = { verticesIds[i], verticesIds[i + 1], verticesIds[i + 2] };
			Node[] vNodes = new Node[3];

			// Inicializacion de los nodos
			for (int j = 0; j < 3; ++j) {
				Vector3 worldPos = transform.TransformPoint (verticesPos[vertices[j]]);
				if (nodes.ContainsKey (worldPos)) {
					vNodes[j] = nodes[worldPos];
					vNodes[j].AddId (vertices[j]);
				} else {
					Vector3 normal = mesh.normals[vertices[j]];
					vNodes[j] = new ClothNode (vertices[j], worldPos, normal, CheckFixers (worldPos), simParams);
					nodes.Add (worldPos, vNodes[j]);
				}
			}

			// Inicializacion del triangulo de nodos
			NodeTriangle tn = new ClothNodeTriangle (vNodes[0], vNodes[1], vNodes[2], (ClothSimulationParameters) simParams);
			triangles.Add (tn);

			// Comprobacion de aristas ya existentes y creacion de los muelles de flexion
			Triangle t = new Triangle (vertices[0], vertices[1], vertices[2]);
			CheckExistingEdges (t, tn, verticesPos);

			// Inicializacion de los muelles
			springs.Add (new ClothSpring (tn.n1, tn.n2, false, simParams));
			springs.Add (new ClothSpring (tn.n1, tn.n3, false, simParams));
			springs.Add (new ClothSpring (tn.n2, tn.n3, false, simParams));
		}

		initialized = true;
	}

	/// <summary>
	/// Checks if an edge has already been visited. In case not, the dictionary is updated. If the opposite, cloth's data structures are updated.
	/// </summary>
	/// <remarks>
	/// In the last case, if simParams.useFlexionSprings is true, a new spring is created. Else, the triangle stores a new neighbour.
	/// THIS METHOD AND THE DATA STRUCTURES IT USES COULD BE IMPROVED.
	/// </remarks>
	/// <param name="t">
	/// A Triangle instance.
	/// </param>
	/// <param name="tn">
	/// A NodeTriangle instance that corresponds to the previous triangle.
	/// </param>
	/// <param name="verticesPos">
	/// An array containing the positions of the vertices of the mesh.
	/// </param>

	private void CheckExistingEdges (Triangle t, NodeTriangle tn, Vector3[] verticesPos) {
		foreach (Edge<int> e in t.edges) {
			switch (simParams.flexionSimulation) {
				case ClothSimulationParameters.FlexionSimulation.Springs:
					if (!edgesFlexionSprings.ContainsKey (e)) {
						edgesFlexionSprings.Add (e, e.other);
					} else {
						int flexionV1 = e.other;
						int flexionV2 = edgesFlexionSprings[e];

						Node nA = nodes[transform.TransformPoint (verticesPos[flexionV1])];
						Node nB = nodes[transform.TransformPoint (verticesPos[flexionV2])];
						springs.Add (new ClothSpring (nA, nB, true, simParams));
					}
					break;

				case ClothSimulationParameters.FlexionSimulation.FaceOrientation:
					if (!edgesFlexion.ContainsKey (e)) {
						edgesFlexion.Add (e, tn);
					} else {
						NodeTriangle neighbour = edgesFlexion[e];

						((ClothNodeTriangle) tn).AddNeighbour (neighbour);
						((ClothNodeTriangle) neighbour).AddNeighbour (tn);
					}
					break;
			}
		}
	}

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
	/// Debug methods
	///

	/// <summary>
	/// Prints in Unity's console the cloth's nodes.
	/// </summary>
	private void DebugNodes () {
		foreach (KeyValuePair<Vector3, Node> entry in nodes) {
			Debug.Log (entry.Value);
		}
	}

	/// <summary>
	/// Prints in Unity's console the cloth's springs.
	/// </summary>
	private void DebugSprings () {
		foreach (Spring s in springs) {
			Debug.Log (s);
		}
	}

	/// <summary>
	/// Prints in Unity's console the cloth's triangles.
	/// </summary>
	private void DebugTriangles () {
		foreach (NodeTriangle t in triangles) {
			Debug.Log (t);
		}
	}

	/// <summary>
	/// Prints in Unity's console the dictionary of cloth's edges.
	/// </summary>
	/// <remarks>
	/// The result will depend on simParams.useFlexionSprings value.
	/// </remarks>
	private void DebugEdges () {
		switch (simParams.flexionSimulation) {
			case ClothSimulationParameters.FlexionSimulation.Springs:
				foreach (KeyValuePair<Edge<int>, int> entry in edgesFlexionSprings) {
					Debug.Log (entry.Key + ": " + entry.Value);
				}
				break;

			case ClothSimulationParameters.FlexionSimulation.FaceOrientation:
				foreach (KeyValuePair<Edge<int>, NodeTriangle> entry in edgesFlexion) {
					Debug.Log (entry.Key + ": " + entry.Value);
				}
				break;
		}
	}
}