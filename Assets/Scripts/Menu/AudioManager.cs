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
        fxSlider.value = Mathf.Min(lastFXVolume, lastMasterVolume); // وابستگی به master
        musicSlider.value = Mathf.Min(lastMusicVolume, lastMasterVolume); // وابستگی به master

        SetMasterVolume(masterSlider.value);
        SetFXVolume(fxSlider.value);
        SetMusicVolume(musicSlider.value);
    }

    public void SetMasterVolume(float value)
    {
        float dB = Mathf.Lerp(-50f, 0f, value);
        audioMixer.SetFloat("MasterVolume", dB);
        lastMasterVolume = value;

        if (!isMasterMuted)
            PlayerPrefs.SetFloat("MasterVolume", value);

        // Force FX and Music sliders to not exceed Master
        if (fxSlider.value > value)
        {
            fxSlider.value = value;
            SetFXVolume(value);
        }

        if (musicSlider.value > value)
        {
            musicSlider.value = value;
            SetMusicVolume(value);
        }
    }

    public void SetFXVolume(float value)
    {
        // محدود کردن مقدار به master
        float clampedValue = Mathf.Min(value, masterSlider.value);
        float dB = Mathf.Lerp(-50f, 0F, clampedValue);
        audioMixer.SetFloat("FXVolume", dB);
        lastFXVolume = clampedValue;

        if (!isFXMuted)
            PlayerPrefs.SetFloat("FXVolume", clampedValue);
    }

    public void SetMusicVolume(float value)
    {
        // محدود کردن مقدار به master
        float clampedValue = Mathf.Min(value, masterSlider.value);
        float dB = Mathf.Lerp(-50f, 0f, clampedValue);
        audioMixer.SetFloat("MusicVolume", dB);
        lastMusicVolume = clampedValue;

        if (!isMusicMuted)
            PlayerPrefs.SetFloat("MusicVolume", clampedValue);
    }

    public void ToggleMuteMaster()
    {
        isMasterMuted = !isMasterMuted;
        if (isMasterMuted)
        {
            audioMixer.SetFloat("MasterVolume", -50f);
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
            audioMixer.SetFloat("FXVolume", -50f);
            fxSlider.value = 0;
        }
        else
        {
            fxSlider.value = lastFXVolume;
            SetFXVolume(lastFXVolume);
        }
    }

    public void ToggleMuteMusic()
    {
        isMusicMuted = !isMusicMuted;
        if (isMusicMuted)
        {
            audioMixer.SetFloat("MusicVolume", -50f);
            musicSlider.value = 0;
        }
        else
        {
            musicSlider.value = lastMusicVolume;
            SetMusicVolume(lastMusicVolume);
        }
    }
}
