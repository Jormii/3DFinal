using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that defines a node of the MassSpringCloth mesh.
/// </summary>
public class ClothNode : Node {

    public Vector3 normal;

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
    public ClothNode (int id, Vector3 worldPosition, Vector3 normal, bool isFixed, ClothSimulationParameters simParams):
        base (id, worldPosition, isFixed, simParams) {
            this.normal = normal.normalized;
        }

    /// <summary>
    /// Calculates and applies the forces that affect this node.
    /// </summary>
    public override void ComputeForce () {
        float nodeMass = ((ClothSimulationParameters)simParams).nodeMass;
        force += nodeMass * simParams.gravity;
        force += -(nodeMass * simParams.nodeDampingFactor) * vel;
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

        return string.Format ("Pos = {0}, Vel = {1}, F = {2}, N = {3}, isFixed = {4}, Ids = {5}",
            pos, vel, force, normal, isFixed, idsString);
    }
}