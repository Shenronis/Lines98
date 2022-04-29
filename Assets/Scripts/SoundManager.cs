using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance {get; private set;}

    [SerializeField] public AudioSource BGM, SFX;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetBGM((PlayerPrefs.GetInt("BGM") == 1));
        SetSFX((PlayerPrefs.GetInt("SFX") == 1));
    }

    public void ToggleBGM()
    {
        BGM.mute = !BGM.mute;
        PlayerPrefs.SetInt("BGM", BGM.mute? 0:1);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        SFX.mute = !SFX.mute;
        PlayerPrefs.SetInt("SFX", SFX.mute? 0:1);
        PlayerPrefs.Save();
    }

    public void SetBGM(bool on)
    {
        if (on) BGM.mute = false;
        else BGM.mute = true;
        
        HUDManager.Instance.SetBGM(on);
        PlayerPrefs.SetInt("BGM", BGM.mute? 0:1);
        PlayerPrefs.Save();
    }

    public void SetSFX(bool on)
    {
        if (on) SFX.mute = false;
        else SFX.mute = true;

        HUDManager.Instance.SetSFX(on);
        PlayerPrefs.SetInt("SFX", SFX.mute? 0:1);
        PlayerPrefs.Save();
    }

    public void PauseBGM()
    {
        BGM.Pause();
    }
    public void UnPauseBGM()
    {
        BGM.UnPause();
    }
}
