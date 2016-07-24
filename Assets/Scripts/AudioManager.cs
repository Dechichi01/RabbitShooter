using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    float masterVolumePercent = 1;
    float sfxVolumePercent = 1;
    float musicVolumePercent = 1;

    AudioSource[] musicSources;
    AudioSource sfx2DSource;
    int activeMusicIndex;

    Transform audioListener;
    Transform playerT;

    SoundLibrary soundLibrary;

    public static AudioManager instance;

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            soundLibrary = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicsSource = new GameObject("Musics source" + (i + 1));
                musicSources[i] = newMusicsSource.AddComponent<AudioSource>();
                newMusicsSource.transform.parent = transform;
            }

            GameObject newSfx2DSource = new GameObject("SFX2D source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            sfx2DSource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            playerT = FindObjectOfType<Player>().transform;

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", masterVolumePercent);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", sfxVolumePercent);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", musicVolumePercent);
        }
        
    }

    void Update()
    {
        if (playerT != null)
            audioListener.position = playerT.position;
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.SFX:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1, bool loop = false)
    {
        activeMusicIndex = 1 - activeMusicIndex;
        musicSources[activeMusicIndex].clip = clip;
        musicSources[activeMusicIndex].loop = loop;
        musicSources[activeMusicIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void PlaySound(string soundName, Vector3 pos)
    {
        PlaySound(soundLibrary.GetClipFromName(soundName), pos);
    }

    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(soundLibrary.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0;

        while(percent <1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }

    public enum AudioChannel { Master, SFX, Music};
}
