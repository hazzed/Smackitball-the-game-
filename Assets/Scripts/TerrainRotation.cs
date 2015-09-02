using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
	void Update() {
		transform.Rotate(Vector3.right * Time.deltaTime);
		transform.Rotate(Vector3.up * Time.deltaTime, Space.World);
	}
}