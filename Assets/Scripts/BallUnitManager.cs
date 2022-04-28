using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallUnitManager : MonoBehaviour
{
    public static BallUnitManager Instance {get; private set;}
    public Ball selectedBallUnit;
    List<Sprite> specialUnitSpriteMasks;
    Queue<Tile> ballUnitQueue;

    void Awake()
    {
        Instance = this;
        ballUnitQueue = new Queue<Tile>();
        specialUnitSpriteMasks = Resources.LoadAll<Sprite>("SpecialUnitMaskSprites").ToList();
    }
    
    public Queue<Tile> GetQueue()
    {
        return ballUnitQueue;
    }
    
    public void SetQueue(Queue<Tile> queue)
    {
        if (ballUnitQueue != null && ballUnitQueue.Count > 0) ballUnitQueue.Clear();        
        ballUnitQueue = queue;
        HUDManager.Instance.SetQueue(ballUnitQueue);
    }

    public void SetBallUnitAt(Tile tile, BallUnitType? color=null, SpecialUnit? special=null, bool isQueueing=false)
    {
        if (tile == null || color == null) return;
        tile.RefreshTile();
        
        GameObject obj = ObjectPooler.Instance.GetFromPool("Ball", shouldSpawn:true);
        Ball unit = obj.GetComponent<Ball>();
        
        // Assign its color!
        unit.Type = (BallUnitType)color;

        // Assign its special
        if (special != null) unit.specialType = (SpecialUnit)special;

        if (!isQueueing) tile.SetBallUnitPosition(unit);
        else tile.SetQueuedBallUnit(unit);
    }

    public void SetBallUnitAt(Vector2 tilePos, BallUnitType? color=null, SpecialUnit? special=null, bool isQueueing=false)
    {
        var tile = GridManager.Instance.GetTileAtPos(tilePos);
        SetBallUnitAt(tile, color, special, isQueueing);
    }

    public void SpawnBallUnits(int count = 3)
    {        
        if (GridManager.Instance.CountAvailableTiles() < count) {
            count = GridManager.Instance.CountAvailableTiles();
        }

        for (int i = 0; i < count; i++)
        {            
            if ((ballUnitQueue != null) && (ballUnitQueue.Count >= 1)) {                
                var nextTile = ballUnitQueue.Dequeue();
                if (!nextTile.OccupiedBallUnit)                            
                {
                    nextTile.SetQueueToBall();
                }
            } else {
                Tile randomTile = GridManager.Instance.GetRandomTilePosition();
                if (randomTile == null) { return; }

                Ball nextUnit = SpawnRandomBallUnit().GetComponent<Ball>();
                randomTile.SetBallUnitPosition(nextUnit, shouldAnimate:false);
            }                                
        }        

        QueueBallUnits();                        
    }
    
    public void QueueBallUnits(int count = 3)
    {        
        if (GridManager.Instance.CountAvailableTiles() < count) {
            count = GridManager.Instance.CountAvailableTiles();
        }

        for (int i = 0; i < count; i++)
        {
            Tile randomTile = GridManager.Instance.GetRandomTilePosition();
            if (randomTile == null) { return; }

            Ball nextUnit = SpawnRandomBallUnit().GetComponent<Ball>();
            
            randomTile.SetQueuedBallUnit(nextUnit);
            ballUnitQueue.Enqueue(randomTile);
        }

        if (GridManager.Instance.CountAvailableTiles() == 0) {
            GameManager.Instance.ChangeState(GameState.Lose);
            return;
        }

        HUDManager.Instance.SetQueue(ballUnitQueue);
        GameManager.Instance.ChangeState(GameState.PlayerTurn);                
    }
    
    public GameObject SpawnRandomBallUnit(float chanceAsSpecial = 0.3f)
    {
        BallUnitType baseColor = GetRandomBallUnit();
        SpecialUnit special = GetRandomSpecial();
        GameObject obj = ObjectPooler.Instance.GetFromPool("Ball", shouldSpawn:true);
        Ball unit = obj.GetComponent<Ball>();        

        // Assign its color!
        unit.Type = baseColor;

        // Chance for the ball to have special
        if (UnityEngine.Random.value <= chanceAsSpecial) {
            // Assign its special!
            if (special != SpecialUnit.None) unit.specialType = special;
        }            

        return obj;
    }

    public BallUnitType GetRandomBallUnit()
    {                
        Type type = typeof(BallUnitType);
        Array values = type.GetEnumValues();
        int index = UnityEngine.Random.Range(0, values.Length);
        BallUnitType ballUnitType = (BallUnitType)values.GetValue(index);
        return ballUnitType;
    }

    public SpecialUnit GetRandomSpecial()
    {
        Type type = typeof(SpecialUnit);
        Array values = type.GetEnumValues();
        int index = UnityEngine.Random.Range(0, values.Length);
        SpecialUnit special = (SpecialUnit)values.GetValue(index);
        return special;
    }

    public Sprite GetSpecialUnitSprite(SpecialUnit special)
    {
        foreach(Sprite sprite in specialUnitSpriteMasks)
        {
            if (sprite.name == special.ToString()) return sprite;
        }
        throw new ArgumentOutOfRangeException(nameof(special), special, null);
    }
}