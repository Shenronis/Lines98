using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is the Ball's base script
/// it contains information about a ball unit's color and special
/// </summary>
public class Ball : MonoBehaviour
{    
    [SerializeField] private ParticleSystem ExplodeFX;
    [SerializeField] private ParticleSystem BombExplodeFX;
    public BallUnitType type;
    public BallUnitType Type {
        set {
            if (Enum.IsDefined(typeof(BallUnitType), value.ToString())) {                
                type = (BallUnitType)Enum.Parse(typeof(BallUnitType), value.ToString());
                SetSpriteColor(type);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        get {
            return type;
        }
    }
    public BallUnitSpecial special;
    public BallUnitSpecial specialType {
        set {
            if (Enum.IsDefined(typeof(BallUnitSpecial), value.ToString())) {                
                special = (BallUnitSpecial)Enum.Parse(typeof(BallUnitSpecial), value.ToString());
                if (special == BallUnitSpecial.None) return;

                Transform mask = this.gameObject.transform.GetChild(0);
                mask.GetComponent<SpriteRenderer>().sprite = BallUnitManager.Instance.GetSpecialUnitSprite(special);
                mask.gameObject.SetActive(true);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        get {
            return special;
        }
    }
    public Tile OccupiedTile;

    public Color GetSpriteColor()
    {        
        return this.GetComponentInParent<SpriteRenderer>().color;
    }

    /// <summary>
    /// Not exactly an overload
    /// but rather the main operation for SetSpriteColor(BallUnitType)
    /// </summary>    
    public void SetSpriteColor(Color color)
    {
        this.GetComponentInParent<SpriteRenderer>().color = color;
    }

    /// <summary>
    /// Set a ball unit's color
    /// </summary>
    /// <param name="type">BallUnitType</param>
    public void SetSpriteColor(BallUnitType type)
    {
        switch (type)
        {
            case BallUnitType.Red: 
                SetSpriteColor(BallUnitColorValue.RED);
                break;
            case BallUnitType.Yellow: 
                SetSpriteColor(BallUnitColorValue.YELLOW);
                break;
            case BallUnitType.Green: 
                SetSpriteColor(BallUnitColorValue.GREEN);
                break;
            case BallUnitType.Cyan: 
                SetSpriteColor(BallUnitColorValue.CYAN);
                break;
            case BallUnitType.Blue: 
                SetSpriteColor(BallUnitColorValue.BLUE);
                break;
            case BallUnitType.Magenta: 
                SetSpriteColor(BallUnitColorValue.MAGENTA);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public Sprite GetMaskSprite()
    {
        return this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
    }

    public SpriteRenderer GetMask()
    {
        return this.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
}

// ===============================
// Ball unit's default values
// ===============================

public class BallUnitSize {
    public static Vector3 NORMAL = new Vector3(0.9f, 0.9f, 1);    
    public static Vector3 QUEUE = new Vector3(0.3f, 0.3f, 1);
}

public class BallUnitColorValue {
    public static Color RED = new Color(255,0,0);
    public static Color YELLOW = new Color(255,255,0);
    public static Color GREEN = new Color(0,255,0);
    public static Color CYAN = new Color(0,255,255);
    public static Color BLUE = new Color(0,0,255);
    public static Color MAGENTA = new Color(255,0,255);
}

public enum BallUnitType
{
    Red,
    Yellow,
    Green,
    Cyan,
    Blue,
    Magenta
}

public enum BallUnitSpecial
{
    None,
    Ghost,
    Bomb,
    Pacman,
    Crate,
    Cardbox
}