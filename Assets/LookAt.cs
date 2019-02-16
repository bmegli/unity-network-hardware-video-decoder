// This complete script can be attached to a camera to make it
// continuously point at another object.

// The target variable shows up as a property in the inspector.
// Drag another object onto it to make the camera look at it.
using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour
{
	public Transform target;

	void Update()
	{
		transform.LookAt(target);
	}
}