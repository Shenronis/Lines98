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

    public void ClearQueue()
    {
        ballUnitQueue.Clear();
    }

    /// <summary>
    /// Set a ball unit or a queueing ball unit at a specific tile
    /// </summary>
    /// <param name="tile">Tile</param>
    /// <param name="color"></param>
    /// <param name="special"></param>
    /// <param name="isQueueing">TRUE if set a queueing ball</param>
    public void SetBallUnitAt(Tile tile, BallUnitType? color=null, BallUnitSpecial? special=null, bool isQueueing=false)
    {
        if (tile == null || color == null) return;
        tile.RefreshTile();
        
        GameObject obj = ObjectPooler.Instance.GetFromPool("Ball", shouldSpawn:true);
        Ball unit = obj.GetComponent<Ball>();
        
        // Assign its color!
        unit.Type = (BallUnitType)color;

        // Assign its special
        if (special != null) unit.specialType = (BallUnitSpecial)special;

        if (!isQueueing) tile.SetBallUnitPosition(unit);
        else tile.SetQueuedBallUnit(unit);
    }

    /// <summary>
    /// Wrapper for SetBallUnitAt(Tile tile, [...])
    /// </summary>
    public void SetBallUnitAt(Vector2 tilePos, BallUnitType? color=null, BallUnitSpecial? special=null, bool isQueueing=false)
    {
        var tile = GridManager.Instance.GetTileAtPos(tilePos);
        SetBallUnitAt(tile, color, special, isQueueing);
    }

    /// <summary>
    /// Spawn ball unit operation
    /// if the ball queue is not empty, proceed to set ball onto tile(s)
    /// else the ball queue is empty, directly spawn ball onto random tile(s)
    /// </summary>
    /// <param name="count">Number of unit to spawn</param>
    public void SpawnBallUnits(int count = 3)
    {        
        if (GridManager.Instance.CountHasNotOccupiedTile() < count) {
            count = GridManager.Instance.CountHasNotOccupiedTile();
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
                Tile randomTile = GridManager.Instance.GetRandomSpawnableTilePosition();
                if (randomTile == null) { return; }

                Ball nextUnit = SpawnRandomBallUnit().GetComponent<Ball>();
                randomTile.SetBallUnitPosition(nextUnit, shouldAnimate:false);
            }                                
        }
        
        QueueBallUnits();                        
    }
    
    /// <summary>
    /// Queueing new ball unit into the queue
    /// after that check for losing condition
    /// in which case, moveable tile==1 else it will allow player to continue loop the game
    /// </summary>
    /// <param name="count">Number of unit to queue</param>
    public void QueueBallUnits(int count = 3)
    {        
        if (GridManager.Instance.CountSpawnableTile() < count) {
            count = GridManager.Instance.CountSpawnableTile();
        }

        for (int i = 0; i < count; i++)
        {
            Tile randomTile = GridManager.Instance.GetRandomSpawnableTilePosition();
            if (randomTile == null) { return; }

            Ball nextUnit = SpawnRandomBallUnit().GetComponent<Ball>();
            
            randomTile.SetQueuedBallUnit(nextUnit);
            ballUnitQueue.Enqueue(randomTile);
        }        

        if (GridManager.Instance.CountMoveableTile() == 1) {
            GameManager.Instance.ChangeState(GameState.Lose);
            return;
        }

        HUDManager.Instance.SetQueue(ballUnitQueue);
        GameManager.Instance.ChangeState(GameState.PlayerTurn);                
    }
    
    /// <summary>
    /// Spawn a ball unit with random color and a chace to randomize it's special ability
    /// </summary>
    /// <param name="chanceAsSpecial">Chance to spawn a unit with special ability</param>
    /// <returns>GameObject</returns>
    public GameObject SpawnRandomBallUnit(float chanceAsSpecial = 0.8f)
    {
        BallUnitType baseColor = GetRandomBallUnit();
        BallUnitSpecial special = GetRandomSpecial();
        GameObject obj = ObjectPooler.Instance.GetFromPool("Ball", shouldSpawn:true);
        Ball unit = obj.GetComponent<Ball>();        

        // Assign its color!
        unit.Type = baseColor;

        // Chance for the ball to have special
        if (UnityEngine.Random.value <= chanceAsSpecial) {
            // Assign its special!
            if (special != BallUnitSpecial.None) unit.specialType = special;
        }            

        return obj;
    }

    /// <summary>
    /// Return a random value from BallUnitType enum
    /// </summary>
    /// <returns>BallUnitType</returns>
    public BallUnitType GetRandomBallUnit()
    {                
        Type type = typeof(BallUnitType);
        Array values = type.GetEnumValues();
        int index = UnityEngine.Random.Range(0, values.Length);
        BallUnitType ballUnitType = (BallUnitType)values.GetValue(index);
        return ballUnitType;
    }

    /// <summary>
    /// Return a random value from BallUnitSpecial
    /// </summary>
    /// <returns>BallUnitSpecial</returns>
    public BallUnitSpecial GetRandomSpecial()
    {    
        Type type = typeof(BallUnitSpecial);
        Array values = type.GetEnumValues();
        int index = UnityEngine.Random.Range(0, values.Length);
        BallUnitSpecial special = (BallUnitSpecial)values.GetValue(index);
        return special;
    }

    /// <summary>
    /// Return Sprite of the special unit
    /// </summary>
    /// <param name="special">BallUnitSpecial</param>
    /// <returns>Sprite</returns>
    public Sprite GetSpecialUnitSprite(BallUnitSpecial special)
    {
        foreach(Sprite sprite in specialUnitSpriteMasks)
        {
            if (sprite.name == special.ToString()) return sprite;
        }
        throw new ArgumentOutOfRangeException(nameof(special), special, null);
    }
}