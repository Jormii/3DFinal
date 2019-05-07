using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node {

	/// <summary>
    /// List that stores the ids of the vertices of the mesh that correspond to the same node.
    /// </summary>
    public List<int> nodeIds;
	public bool isFixed;
	public Vector3 pos;
    public Vector3 vel;
    public Vector3 force;
	public Matrix4x4 dF_dx;
	public SimulationParameters simParams;

	/// <summary>
    /// Initializes a Node instance.
    /// </summary>
    /// <param name="id">
    /// The id of the vertex that this node represents.
    /// </param>
    /// <param name="worldPosition">
    /// The position of the node in world coordinates.
    /// </param>
    /// <param name="normal">
    /// The normal of the node in world coordinates. The initialized node will store this value normalized.
    /// </param>
    /// <param name="isFixed">
    /// Represents if the node should update its position and velocity in each frame.
    /// </param>
    /// <param name="simParams">
    /// A reference to the instance that holds the parameters of the simulation.
    /// </param>
    public Node (int id, Vector3 worldPosition, bool isFixed, SimulationParameters simParams) {
        this.nodeIds = new List<int> () {
            id
        };
        this.isFixed = isFixed;
        this.pos = worldPosition;
        this.vel = Vector3.zero;
        this.force = Vector3.zero;
        this.dF_dx = Matrix4x4.zero;
		this.simParams = simParams;
    }

	/// <summary>
    /// Adds a vertex id to the node's list unless it already contains it.
    /// </summary>
    /// <param name="id">
    /// The vertex id.
    /// </param>
    public void AddId (int id) {
        if (!nodeIds.Contains (id)) {
            nodeIds.Add (id);
        }
    }

	/// <summary>
    /// Resets node's variables for the next iteration.
    /// </summary>
    public void Reset()
    {
        force = Vector3.zero;
        dF_dx = Matrix4x4.zero;
    }

	/// <summary>
    /// Calculates and applies the forces that affect this node.
    /// </summary>
    public abstract void ComputeForce ();

	/// <summary>
    /// Calculates the penalty force and applies it to the node.
    /// </summary>
    /// <remarks>
    /// This method is called when the node collides with an object that holds any Penalty component.
    /// </remarks>
    /// <param name="penalty">
    /// The Penalty component of the object the node collided with.
    /// </param>
    public void ApplyPenaltyForce (Penalty penalty) {
        dF_dx = penalty.CalculatePenaltyMatrix (this);
    }

	/// <summary>
    /// Checks if two nodes are equal.
    /// </summary>
    /// <remarks>
    /// Two nodes are equal if their positions are the same.
    /// </remarks>
    /// <param name="obj">
    /// The node to compare.
    /// </param>
    /// <returns>
    /// Whether or not the nodes meet the aforementioned condition.
    /// </returns>
    public override bool Equals (object obj) {
        var otherNode = obj as Node;
        if (otherNode == null) {
            return false;
        }

        return pos.Equals (otherNode.pos);
    }

	/// <summary>
    /// A hash code for the current node.
    /// </summary>
    /// <returns>
    /// The node's hash code.
    /// </returns>
    public override int GetHashCode () {
        return pos.GetHashCode ();
    }
}
