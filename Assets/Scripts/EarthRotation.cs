using UnityEngine;
using System.Collections;

public class EarthRotation : MonoBehaviour {
	void Update() {

		transform.Rotate(Vector3.up, 1 * Time.deltaTime, Space.World);
	}
}