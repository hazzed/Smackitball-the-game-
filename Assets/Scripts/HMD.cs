using UnityEngine;
using System.Collections;

public class HMD : MonoBehaviour {
	
	//public head gameObject;
	public Transform headBone = null;
	public Transform cameraObject = null;
	public Vector3 neckCorrectionEuler;
	
	
	void LateUpdate()
	{
		headBone.rotation = cameraObject.rotation * Quaternion.Euler (neckCorrectionEuler);
	}
}