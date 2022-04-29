using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : MonoBehaviour
{    
    public static GameManager Instance {get; private set;}
    public int score;
    public int highscore;
    public int Score {
        set {
            score = value;
            HUDManager.Instance.SetScore(score);
            
            // Highscore
            if (score > highscore)
            {
                highscore = score;
                PlayerPrefs.SetInt("Highscore", highscore);
                HUDManager.Instance.SetHighscore(highscore);
                PlayerPrefs.Save();
            }
        }

        get {
            return score;
        }
    }
    public float time;
    public float Time {
        set {
            time = value;
        }

        get {
            return score;
        }
    }
    public GameState gameState;
    string savePath;

    void Awake()
    {
        Instance = this;
        savePath = Application.persistentDataPath + "/save.json";
    }    

    void Start()
    {        
        ChangeState(GameState.GenerateGrid);

        highscore = PlayerPrefs.GetInt("Highscore", 0);
        HUDManager.Instance.SetHighscore(highscore);

		// Check if we need to load game
        if (PlayerPrefs.GetInt("Load") == 1) LoadGame();
        PlayerPrefs.SetInt("Load", 0);
        PlayerPrefs.Save();
    }

    public void ChangeState(GameState state)
    {        
        gameState = state;
        switch (state)
        {
            // Starting State or Restarting State
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;

            // Spawn and then Queue State
            case GameState.SpawnAndQueue:
                BallUnitManager.Instance.SpawnBallUnits();                
                break;

            // Player's turn, all handled in Tile.cs
            case GameState.PlayerTurn:
                break;

            // Losing State
            case GameState.Lose:                
                PauseMenu.Instance.ChangePromptText("YOU LOSE!");
                PauseMenu.Instance.OpenPopup(); // closing popup will then restart the game
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }    

    public SaveState CreateSaveState()
    {
        SaveState save = new SaveState();
        save.gameState = (Int32)gameState;                              // Current game's state
        save.score = score;                                             // Current score
        save.time = time;                                               // Current time            

        // Save grid (Gameboard) state
        foreach(var cell in GridManager.Instance.GetGrid())
        {
            var tilePos = (Vector2)cell.Key;
            var unit = cell.Value.OccupiedBallUnit;

            if (unit != null) 
            {
                save.board[tilePos.ToString()] = new Tuple<BallUnitType, BallUnitSpecial>(unit.Type, unit.specialType);
            }
        }

        // Save pending queue
        foreach(var tile in BallUnitManager.Instance.GetQueue())
        {
            var tilePos = (Vector2)tile.transform.position;
            var queuedUnit = tile.queuedBallIndicator;

            if (queuedUnit != null)
            {
                save.queue[tilePos.ToString()] = new Tuple<BallUnitType, BallUnitSpecial>(queuedUnit.Type, queuedUnit.specialType);
            }
        }

        return save;
    }

    /// <summary>
    /// Save the current game
    /// </summary>
    /// <returns>TRUE if saved</returns>
    public bool SaveGame()
    {        
        SaveState save = CreateSaveState();
        string json = JsonConvert.SerializeObject(save);

        try
        {
            using (StreamWriter file = File.CreateText(savePath))
            {
                JsonSerializer serializer = new JsonSerializer();            
                serializer.Serialize(file, save);
            }
        }
        catch (IOException ex)
        {
            Debug.Log(ex.StackTrace);
            return false;
        }

        return true;
    }

    public SaveState LoadSaveState()
    {
        SaveState save;
        try {
            using (StreamReader file = File.OpenText(savePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                save = (SaveState)serializer.Deserialize(file, typeof(SaveState));
            }
        } 
        catch (IOException ex)
        {
            save = null;
            Debug.Log(ex.StackTrace);
        }        

        return save;
    }

    public void LoadGame()
    {
        SaveState save = LoadSaveState();        
        if (save == null) return;

        GridManager.Instance.RestartAllTiles();
        ChangeState((GameState)save.gameState); // Game state                   
        Score = save.score;                     // Score
        Timer.Instance.SetTime(save.time);      // Time
                
        // Grid (Gameboard)
        foreach(var pair in save.board)
        {
            Vector2 pos = ParseStringToVector2(pair.Key);
            BallUnitType type = (BallUnitType)pair.Value.Item1;
            BallUnitSpecial special = (BallUnitSpecial)pair.Value.Item2;
                        
            BallUnitManager.Instance.SetBallUnitAt(pos, type, special, isQueueing:false);
        }
        
        // Queue
        Queue<Tile> queue = new Queue<Tile>();
        foreach(var pair in save.queue)
        {
            Vector2 pos = ParseStringToVector2(pair.Key);
            BallUnitType type = (BallUnitType)pair.Value.Item1;
            BallUnitSpecial special = (BallUnitSpecial)pair.Value.Item2;            

            // Spawn as queue
            BallUnitManager.Instance.SetBallUnitAt(pos, type, special, isQueueing:true);

            // Restore queue
            queue.Enqueue(GridManager.Instance.GetTileAtPos(pos));
        }        
        // Assign queue
        BallUnitManager.Instance.SetQueue(queue);
    }

    /// <summary>
    /// Parse string to Vector2
    /// </summary>
    /// <param name="strVector">"(x, y)"</param>
    /// <returns>Vector2</returns>
    private Vector2 ParseStringToVector2(string strVector)
    {
        // RegEx for matching coords 
        // Capture group#1 -> x
        // Capture group#3 -> y
        var pattern = @"(-?\d+(\.\d+)?),\s*(-?\d+(\.\d+)?)";
        var match = Regex.Match(strVector, pattern);
        float x = float.Parse(match.Groups[1].ToString());
        float y = float.Parse(match.Groups[3].ToString());
        return new Vector2(x,y);
    }
}

public enum GameState
{    
    GenerateGrid,
    SpawnAndQueue,
    PlayerTurn,    
    Lose
}