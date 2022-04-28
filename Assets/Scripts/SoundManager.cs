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

    public void ToggleBGM()
    {
        BGM.mute = !BGM.mute;
    }

    public void ToggleSFX()
    {
        SFX.mute = !SFX.mute;
    }

    public void SetBGM(bool on)
    {
        if (on) BGM.mute = false;
        else BGM.mute = true;
        
        HUDManager.Instance.SetBGM(on);
    }

    public void SetSFX(bool on)
    {
        if (on) SFX.mute = false;
        else SFX.mute = true;

        HUDManager.Instance.SetSFX(on);
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
