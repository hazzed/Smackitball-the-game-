using UnityEngine;
using System.Collections;

public class CreateGameButton : MonoBehaviour {
	
	public void ButtonCreateGame(int index)
	{
		Application.LoadLevel (2);
	}
	
	public void ButtonCreateGame(string CreateRoom)
	{
		Application.LoadLevel (CreateRoom);
	}
}