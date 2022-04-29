using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveState
{
    public int gameState;
    public int score;
    public float time;

    /*
        Why string and Tuple<>
        I want to save everything in .json

        Saving as in form of Dictionary<Vector2Int, Ball>
            so type string would represent Vector2Int as "({x}, {y})"
            and type Tuple<> would represent a Ball class (color & special)
    */
    public Dictionary<string, Tuple<BallUnitType, BallUnitSpecial>> board = new Dictionary<string, Tuple<BallUnitType, BallUnitSpecial>>();
    public Dictionary<string, Tuple<BallUnitType, BallUnitSpecial>> queue = new Dictionary<string, Tuple<BallUnitType, BallUnitSpecial>>();
}
