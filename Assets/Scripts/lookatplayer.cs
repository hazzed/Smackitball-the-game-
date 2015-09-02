using UnityEngine;
using System.Collections;

public class lookatplayer : MonoBehaviour {
	public Transform target;
	void Update() {
		transform.LookAt(target);
	}
}