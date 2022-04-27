using System;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    public static SoundEffectPlayer Instance {get; private set;}
    [SerializeField] AudioClip ballUnitExplode;
    [SerializeField] AudioClip ghostMove;
    [SerializeField] AudioClip bombMove;
    [SerializeField] AudioClip bombSpecialExplode;    
    [SerializeField] AudioClip moveSFX;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(SFX track)
    {
        switch(track)
        {
            case SFX.pop:
                audioSource.PlayOneShot(ballUnitExplode);
                break;
            case SFX.explosion:
                audioSource.PlayOneShot(bombSpecialExplode);
                break;
            case SFX.ghost:
                audioSource.PlayOneShot(ghostMove);
                break;
            case SFX.bomb:
                audioSource.PlayOneShot(bombMove);
                break;
            case SFX.move:
                audioSource.PlayOneShot(moveSFX);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(track), track, null);
        }
    }
}

public enum SFX
{
    pop,
    ghost,
    bomb,
    explosion,
    move
}