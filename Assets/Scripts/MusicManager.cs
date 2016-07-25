using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour {

    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string sceneName;

	void Start () {
        //AudioManager.instance.PlayMusic(menuTheme, 2f, true);
        OnLevelWasLoaded(0);
	}	

    void OnLevelWasLoaded(int sceneIndex)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName !=sceneName)
        {
            sceneName = newSceneName;
            Invoke("PlayMusic", .2f);
        }
    }

    void PlayMusic()
    {
        AudioClip clipToPlay = null;
        switch(sceneName)
        {
            case "Start":
                clipToPlay = menuTheme;
                break;
            case "Game":
                clipToPlay = mainTheme;
                break;
            default:
                clipToPlay = mainTheme;
                break;
        }

        AudioManager.instance.PlayMusic(clipToPlay, 2, true);
    }
	
	void Update () {
	
	}
}
