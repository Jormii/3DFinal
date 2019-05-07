using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple component that fixes those MassSpringCloth nodes that collide with this object.
/// </summary>
/// <remarks>
/// In order to achieve this behaviour, the fixer must be a child of a MassSpringCloth object.
/// </remarks>
public class Fixer : MonoBehaviour {

	/// <summary>
	/// On game start, this gameobject's tag is set to "Fixer".
	/// </summary>
	void Awake () {
		gameObject.tag = "Fixer";
	}
}