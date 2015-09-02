using UnityEngine;

public class RandomMatchmaker : MonoBehaviour
{


	// Use this for initialization
	void Start()
	{


		PhotonNetwork.ConnectUsingSettings("0.1");
	}



	void OnGUI()
	{
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
	}


	void OnJoinedLobby()
	{
		PhotonNetwork.JoinRandomRoom ();
		GameObject player = PhotonNetwork.Instantiate ("player", Vector3.zero, Quaternion.identity, 0);
		PhotonView myPhotonView = (PhotonView)player.GetComponent<PhotonView> ();

		}



	void OnPhotonRandomJoinFailed()
	{
		Debug.Log("Can't join random room!");
		PhotonNetwork.CreateRoom(null);
	
	}


	void OnJoinedRoom()
		{
		GameObject player = PhotonNetwork.Instantiate ("player", Vector3.zero, Quaternion.identity, 0);
		PhotonView myPhotonView = (PhotonView)player.GetComponent<PhotonView> ();
		
		} 
		
	
	



}

















































