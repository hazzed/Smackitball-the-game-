using UnityEngine;
using System.Collections;

public class BallBehaviorScript : MonoBehaviour {
	//NOTE TO DYLAN :: make this an int later cause its much more flexible than a bool
	//bools suck
	private bool bounce;
	private float speed;
	
	private Vector3 initPos;
	public Vector3 InitialImpulse;
	
	public LayerMask layerMask; //make sure we aren't in this layer 
	public float skinWidth = 0.1f; //probably doesn't need to be changed 
	
	private float minimumExtent; 
	private float partialExtent; 
	private float sqrMinimumExtent; 
	private Vector3 previousPosition;

	private int waitCount = 0;

	public float swingVelocity = 30f;
	
	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody>().AddForce (InitialImpulse, ForceMode.Impulse);
	}
	
	//initialize values 
	void Awake() 
	{ 
		initPos = transform.position;
		bounce = false;
		previousPosition = GetComponent<Rigidbody>().position; 
		minimumExtent = Mathf.Min(Mathf.Min(GetComponent<Collider>().bounds.extents.x, GetComponent<Collider>().bounds.extents.y), GetComponent<Collider>().bounds.extents.z); 
		partialExtent = minimumExtent * (1.0f - skinWidth); 
		sqrMinimumExtent = minimumExtent * minimumExtent; 
	} 
	
	void FixedUpdate()
	{	
		//have we moved more than our minimum extent? 
		Vector3 movementThisStep = GetComponent<Rigidbody>().position - previousPosition; 
		float movementSqrMagnitude = movementThisStep.sqrMagnitude;

		waitCount = waitCount < 20 ? (waitCount == 0 ? 0 : waitCount++) : 0;
		
		if (movementSqrMagnitude > sqrMinimumExtent) 
		{ 
			float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
			RaycastHit hitInfo; 
			
			//check for obstructions we might have missed 
			if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementMagnitude, layerMask.value)) 
				GetComponent<Rigidbody>().position = hitInfo.point - (movementThisStep/movementMagnitude)*partialExtent; 
		} 
		
		previousPosition = GetComponent<Rigidbody>().position; 
	}
	
	void OnCollisionEnter(Collision target)
	{
		if (target.gameObject.name == "Racket" || target.gameObject.name == "ReceivingRacket" || target.gameObject.name == "ServingRacket") 
		{/*
			bounce = false;
			speed = target.relativeVelocity.magnitude;
			Debug.Log (speed);
			if(speed < 20)
			{
				speed = rigidbody.velocity.magnitude*100;
				Debug.Log (speed);
				Vector3 dir = (transform.position - target.transform.position).normalized;
				dir.y = dir.y + 1f;
				Debug.Log (dir);
				rigidbody.AddForce (speed*dir);
			}*/
			Vector3 v =  (target.transform.rotation * Vector3.forward).normalized * swingVelocity * -1f;
			GetComponent<Rigidbody>().AddForce(-GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
			GetComponent<Rigidbody>().AddForce(v, ForceMode.VelocityChange);
			GetComponent<Rigidbody>().AddForce(target.transform.rotation * Vector3.up* 12f, ForceMode.VelocityChange);
			transform.position +=v/swingVelocity*2f;
			waitCount++;
		}
		
		else if(target.gameObject.name == "Floor")
		{
			if(bounce)
			{
				bounce = false;
				Debug.Log ("Game Over");
				GetComponent<Rigidbody>().MovePosition(initPos);
				GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
			}
			
			else
				bounce = true;
		}
	}
}