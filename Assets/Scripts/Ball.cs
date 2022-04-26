using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
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
    PacMan
}