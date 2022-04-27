using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance {get; private set;}
    void Awake()
    {
        Instance = this;
    }
    #endregion

    public int score;
    public int Score {
        set {
            score = value;
            HUDManager.Instance.SetScore(score);
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

    void Start()
    {
        ChangeState(GameState.GenerateGrid);
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
    
}

public enum GameState
{
    GenerateGrid,
    SpawnAndQueue,
    PlayerTurn,
    Win,
    Lose
}