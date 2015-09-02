using UnityEngine;


public class JoinRoom: MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
		PhotonNetwork.ConnectUsingSettings ("0.1");
		Debug.Log ("Void Start (): Connected to master");
	}
	
	
	void OnJoinedLobby()
	{
		PhotonNetwork.JoinRandomRoom ();
		Debug.Log ("OnJoinedLobby(): Joined Lobby");
	}
	
	
	public void OnJoinedRoom()
	{
		GameObject player = PhotonNetwork.Instantiate ("player", Vector3.zero, Quaternion.identity, 0);
		PhotonView myPhotonView = (PhotonView)player.GetComponent<PhotonView> ();
		Debug.Log("OnJoinedRoom(): Spawned");
		
		OVRPlayerController controller = player.GetComponent<OVRPlayerController> ();
		controller.enabled = true;
		
		GameObject.Find ("Camera").GetComponentInChildren<OVRCameraRig> ().enabled = true;
	}
}