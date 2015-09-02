


using UnityEngine;

public class CreateRoom : MonoBehaviour
{
	
	
	// Use this for initialization
	void Start()
	{
		
		
		PhotonNetwork.ConnectUsingSettings("0.1");


	}

	void OnJoinedLobby()
	{
		PhotonNetwork.CreateRoom (null, true, true, 4);

	}


	public void OnJoinedRoom()
	{
		GameObject player = PhotonNetwork.Instantiate ("player", Vector3.zero, Quaternion.identity, 0);
		PhotonView myPhotonView = (PhotonView)player.GetComponent<PhotonView> ();
		Debug.Log ("OnJoinedRoom(): Spawned");

		OVRPlayerController controller = player.GetComponent<OVRPlayerController> ();
		controller.enabled = true;


		GameObject.Find ("Camera").GetComponentInChildren<OVRCameraRig> ().enabled = true;
		}

	

	
		}
	






