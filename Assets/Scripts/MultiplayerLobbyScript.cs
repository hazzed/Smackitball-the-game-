using UnityEngine;
using System.Collections;

public class MultiplayerLobbyScript : MonoBehaviour {
	
	public void ButtonMultiplayerLobby(int index)
	{
		Application.LoadLevel (4);
	}
	
	public void ButtonMultiplayerLobby(string Blankscene)
	{
		Application.LoadLevel (Blankscene);
	}
}