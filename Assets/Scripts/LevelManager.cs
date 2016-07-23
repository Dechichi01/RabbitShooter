using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

	public void LoadLevel(string name){
		Debug.Log ("Level load requested for: "+name);
		Application.LoadLevel(name);
	}
	
	public void QuitRequest (){
		Debug.Log ("Quitting...");
		Application.Quit(); //Has no effect in Web, Mobile and Debug Mode
	}
	
	public void LoadNextLevel(){
		Application.LoadLevel(Application.loadedLevel + 1);
	}
	
}
