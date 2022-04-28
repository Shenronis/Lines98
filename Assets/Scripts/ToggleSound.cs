using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSound : MonoBehaviour
{
    [SerializeField] private bool toggleBGM, toggleSFX;

    public void Toggle()
    {
        if (toggleBGM) SoundManager.Instance.ToggleBGM();
        if (toggleSFX) SoundManager.Instance.ToggleSFX();
    }
}
