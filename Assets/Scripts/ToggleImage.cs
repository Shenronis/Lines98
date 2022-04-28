using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleImage : MonoBehaviour
{
    [SerializeField] private bool BGM, SFX;

    public void Toggle()
    {
        if (BGM) HUDManager.Instance.ToggleBGM();
        if (SFX) HUDManager.Instance.ToggleSFX();
    }
}
