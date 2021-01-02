using UnityEngine;

using System.Collections;
using PlayerControls;


public class CameraLook : MonoBehaviour 
{
	public Transform target;

	void Start ()
	{
		target = PlayerMovement.instance.cameraAnimator.transform;
	}

	void Update () 
	{
		Vector3  v = target.position  - transform.position ;

		v.x = v.z = 0.0f;
		transform.LookAt (target.position  - v); 
	}
}