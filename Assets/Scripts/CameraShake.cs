using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance {get; private set;}
    Camera cam;    
    Vector3 originPosition;
    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    /*
        The reason why is, if multiple shake was setted off and we don't have the origin position
        the camera will start another DOShakePosition with a initial somewhere in "was shaking" position.

        I fix it by saving this origin and pan the camera back on complete.
    */
    public void SetCameraOrigin(Vector3 pos)
    {
        originPosition = pos;
    }

    public void Shake(float duration=0.5f, float strength=0.5f)
    {        
        cam.DOShakePosition(duration, strength:1f)
            .OnComplete( () => {
                cam.transform.DOMove(originPosition, strength);
            });
    }
}
