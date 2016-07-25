using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject optionsMenu;

    public Slider[] volumeSliders;

    public void Start()
    {
        if (mainMenu !=null && optionsMenu !=null)
        {
            MainMenu();
            volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
            volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
            volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;
        }        
    }

    public void LoadLevel(string name){
		SceneManager.LoadScene(name);
	}
	
	public void QuitRequest (){
		Application.Quit(); //Has no effect in Web, Mobile and Debug Mode
	}
	
	public void LoadNextLevel(){
		SceneManager.LoadScene(Application.loadedLevel + 1);
	}

    public void OptionsMenu()
    {
        if (mainMenu != null)
            mainMenu.SetActive(false);
        if (optionsMenu != null)
            optionsMenu.SetActive(true);
    }

    public void MainMenu()
    {
        if (mainMenu !=null)
            mainMenu.SetActive(true);
        if (optionsMenu !=null)
            optionsMenu.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.SFX);
    }
    
}
