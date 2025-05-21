using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    private const string PLAYER_PREF_MUSIC_VOLUME ="MusicVolume"; 
    public static MusicManager Instance;
    private AudioSource musicSource;
    private float volume;

    private void Awake()
    {
        Instance = this;
        musicSource = GetComponent<AudioSource>();
        volume = PlayerPrefs.GetFloat(PLAYER_PREF_MUSIC_VOLUME,0f);
        musicSource.volume = volume;
    }
    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0f;
        }
        musicSource.volume = volume;
        PlayerPrefs.SetFloat(PLAYER_PREF_MUSIC_VOLUME, volume);
    }
    public float GetVolume()
    {
        return volume;
    }
}
