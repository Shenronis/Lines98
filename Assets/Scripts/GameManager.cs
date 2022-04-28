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

        if (PlayerPrefs.GetInt("Load") == 1) LoadGame();
        PlayerPrefs.SetInt("Load", 0);        
    }

    public void ChangeState(GameState state)
    {        
        gameState = state;
        switch (state)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnAndQueue:
                BallUnitManager.Instance.SpawnBallUnits();                
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.Lose:
                GridManager.Instance.RestartAllTiles();
                ChangeState(GameState.GenerateGrid);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }    

    public SaveState CreateSaveState()
    {
        SaveState save = new SaveState();
        save.gameState = (Int32)gameState;
        save.score = score;
        save.time = time;

        foreach(var cell in GridManager.Instance.GetGrid())
        {
            var tilePos = (Vector2)cell.Key;
            var unit = cell.Value.OccupiedBallUnit;

            if (unit != null) 
            {
                save.board[tilePos.ToString()] = new Tuple<BallUnitType, SpecialUnit>(unit.Type, unit.specialType);
            }
        }

        foreach(var tile in BallUnitManager.Instance.GetQueue())
        {
            var tilePos = (Vector2)tile.transform.position;
            var queuedUnit = tile.queuedBallIndicator;

            if (queuedUnit != null)
            {
                save.queue[tilePos.ToString()] = new Tuple<BallUnitType, SpecialUnit>(queuedUnit.Type, queuedUnit.specialType);
            }
        }

        return save;
    }

    public bool SaveGame()
    {        
        SaveState save = CreateSaveState();
        string json = JsonConvert.SerializeObject(save);

        PlayerPrefs.SetInt("BGM", SoundManager.Instance.BGM.mute? 0:1);
        PlayerPrefs.SetInt("SFX", SoundManager.Instance.SFX.mute? 0:1);
        PlayerPrefs.Save();

        // Debug
        // Debug.Log(json);
        // GridManager.Instance.RestartAllTiles();
        // ~Debug

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

        SoundManager.Instance.SetBGM((PlayerPrefs.GetInt("BGM") == 1));
        SoundManager.Instance.SetSFX((PlayerPrefs.GetInt("SFX") == 1));        

        GridManager.Instance.RestartAllTiles();
        ChangeState((GameState)save.gameState);
        
        Debug.Log($"Score: {save.score}");        
        Score = save.score;

        Debug.Log($"Time: {save.time}");
        Timer.Instance.SetTime(save.time);
        
        Debug.Log("Board");
        foreach(var pair in save.board)
        {
            Vector2 pos = ParseStringToVector2(pair.Key);
            BallUnitType type = (BallUnitType)pair.Value.Item1;
            SpecialUnit special = (SpecialUnit)pair.Value.Item2;
            Debug.Log($"Tile({pos.x}, {pos.y}): {type.ToString()} - {special.ToString()}");
            
            BallUnitManager.Instance.SetBallUnitAt(pos, type, special, isQueueing:false);
        }
        
        Debug.Log("Queue");
        Queue<Tile> queue = new Queue<Tile>();
        foreach(var pair in save.queue)
        {
            Vector2 pos = ParseStringToVector2(pair.Key);
            BallUnitType type = (BallUnitType)pair.Value.Item1;
            SpecialUnit special = (SpecialUnit)pair.Value.Item2;
            Debug.Log($"Tile({pos.x}, {pos.y}): {type.ToString()} - {special.ToString()}");

            BallUnitManager.Instance.SetBallUnitAt(pos, type, special, isQueueing:true);
            queue.Enqueue(GridManager.Instance.GetTileAtPos(pos));
        }
        BallUnitManager.Instance.SetQueue(queue);
    }

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
    Win,
    Lose
}