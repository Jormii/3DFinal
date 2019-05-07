using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that defines a node of the RigidBody tetrahedron mesh.
/// </summary>
public class RigidBodyNode : Node {

    public float mass;

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
    public RigidBodyNode (int id, Vector3 worldPosition, bool isFixed, SimulationParameters simParams):
        base (id, worldPosition, isFixed, simParams) {
            this.mass = 0.0f;
        }

    /// <summary>
    /// Calculates and applies the forces that affect this node.
    /// </summary>
    public override void ComputeForce () {
        force += mass * simParams.gravity;
        force += -(mass * simParams.nodeDampingFactor) * vel;
    }

    /// <summary>
    /// Returns a string that represents the node.
    /// </summary>
    /// <returns>
    /// Said string.
    /// </returns>
    public override string ToString () {
        string idsString = "[";
        foreach (int id in nodeIds) {
            idsString += " " + id;
        }
        idsString += " ]";

        return string.Format ("Pos: {0}, Vel: {1}, F: {2}, isFixed: {3}, Ids: {4}",
            pos, vel, force, isFixed, idsString);
    }
}