using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider fxSlider;
    public Slider musicSlider;

    private float lastMasterVolume = 0.75f;
    private float lastFXVolume = 0.75f;
    private float lastMusicVolume = 0.75f;

    private bool isMasterMuted = false;
    private bool isFXMuted = false;
    private bool isMusicMuted = false;

    void Start()
    {
        // Load saved values
        lastMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        lastFXVolume = PlayerPrefs.GetFloat("FXVolume", 0.75f);
        lastMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);

        masterSlider.value = lastMasterVolume;
        fxSlider.value = lastFXVolume;
        musicSlider.value = lastMusicVolume;

        SetMasterVolume(masterSlider.value);
        SetFXVolume(fxSlider.value);
        SetMusicVolume(musicSlider.value);
    }

    public void SetMasterVolume(float value)
    {
        float dB = Mathf.Lerp(-80f, 10f, value); // Max +10dB
        audioMixer.SetFloat("MasterVolume", dB);
        lastMasterVolume = value;
        if (!isMasterMuted) PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetFXVolume(float value)
    {
        float dB = Mathf.Lerp(-80f, 10f, value); // Max +10dB
        audioMixer.SetFloat("FXVolume", dB);
        lastFXVolume = value;
        if (!isFXMuted) PlayerPrefs.SetFloat("FXVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        float dB = Mathf.Lerp(-80f, -10f, value); // Max -10dB
        audioMixer.SetFloat("MusicVolume", dB);
        lastMusicVolume = value;
        if (!isMusicMuted) PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void ToggleMuteMaster()
    {
        isMasterMuted = !isMasterMuted;
        if (isMasterMuted)
        {
            audioMixer.SetFloat("MasterVolume", -80f);
            masterSlider.value = 0;
        }
        else
        {
            SetMasterVolume(lastMasterVolume);
            masterSlider.value = lastMasterVolume;
        }
    }

    public void ToggleMuteFX()
    {
        isFXMuted = !isFXMuted;
        if (isFXMuted)
        {
            audioMixer.SetFloat("FXVolume", -80f);
            fxSlider.value = 0;
        }
        else
        {
            SetFXVolume(lastFXVolume);
            fxSlider.value = lastFXVolume;
        }
    }

    public void ToggleMuteMusic()
    {
        isMusicMuted = !isMusicMuted;
        if (isMusicMuted)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
            musicSlider.value = 0;
        }
        else
        {
            SetMusicVolume(lastMusicVolume);
            musicSlider.value = lastMusicVolume;
        }
    }
}
