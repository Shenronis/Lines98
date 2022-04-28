using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveState
{
    public int gameState;
    public int score;
    public float time;
    public Dictionary<string, Tuple<BallUnitType, SpecialUnit>> board = new Dictionary<string, Tuple<BallUnitType, SpecialUnit>>();
    public Dictionary<string, Tuple<BallUnitType, SpecialUnit>> queue = new Dictionary<string, Tuple<BallUnitType, SpecialUnit>>();
}
