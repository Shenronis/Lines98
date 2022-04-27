using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private ParticleSystem ExplodeFX;
    [SerializeField] private ParticleSystem BombExplodeFX;
    public BallUnitType type;
    public BallUnitType Type {
        set {
            if (Enum.IsDefined(typeof(BallUnitType), value.ToString())) {                
                type = (BallUnitType)Enum.Parse(typeof(BallUnitType), value.ToString());
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
    public SpecialUnit special;
    public SpecialUnit specialType {
        set {
            if (Enum.IsDefined(typeof(SpecialUnit), value.ToString())) {                
                special = (SpecialUnit)Enum.Parse(typeof(SpecialUnit), value.ToString());
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

    public Sprite GetSprite()
    {
        return this.GetComponentInParent<SpriteRenderer>().sprite;
    }

    public Sprite GetMask()
    {
        return this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
    }
}

public class BallUnitSize {
    public static Vector3 NORMAL = new Vector3(0.9f, 0.9f, 1);    
    public static Vector3 QUEUE = new Vector3(0.3f, 0.3f, 1);
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

public enum SpecialUnit
{
    None,
    Ghost,
    Bomb,
    // PacMan
}