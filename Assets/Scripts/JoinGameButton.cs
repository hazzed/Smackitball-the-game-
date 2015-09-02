using UnityEngine;
using UnityEngine;
using System.Collections;

public class JoinGameButton : MonoBehaviour {
	
	public void ButtonJoinGame(int index)
	{
		Application.LoadLevel (3);
	}
	
	public void ButtonJoin(string JoinRoom)
	{
		Application.LoadLevel (JoinRoom);
	}
}