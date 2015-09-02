using UnityEngine;
using System.Collections;

public class SinglePlayerButton : MonoBehaviour {

	public void ButtonSinglePlayer(int index)
	{
		Application.LoadLevel (1);
	}

	public void ButtonSinglePlayer(string Singleplayer)
	{
		Application.LoadLevel (Singleplayer);
	}
}






















	