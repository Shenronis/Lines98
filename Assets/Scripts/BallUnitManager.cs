using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallUnitManager : MonoBehaviour
{
    public static BallUnitManager Instance {get; private set;}
    public Ball selectedBallUnit;
    public List<Sprite> specialUnitSpriteMasks;
    Queue<Tile> ballUnitQueue;

    void Awake()
    {
        Instance = this;
        ballUnitQueue = new Queue<Tile>();
        specialUnitSpriteMasks = Resources.LoadAll<Sprite>("SpecialUnitMaskSprites").ToList();
    }

    public void SpawnBallUnits(int count = 3)
    {        
        for (int i = 0; i < count; i++)
        {            
            if ((ballUnitQueue != null) && (ballUnitQueue.Count >= 1)) {
                var nextTile = ballUnitQueue.Dequeue();
                if (!nextTile.OccupiedBallUnit)                            
                {
                    nextTile.SetQueueToBall();
                }
            } else {           
                if (GridManager.Instance.CountAvailableTiles() < (count - i + ballUnitQueue.Count)) {
                    GameManager.Instance.ChangeState(GameState.Lose);
                    return;
                }

                Tile randomTile = GridManager.Instance.GetRandomTilePosition();
                if (randomTile == null) { return; }

                GameObject unitToSpawn = GetRandomBallUnit();
                randomTile.SetBallUnitPosition(unitToSpawn.GetComponent<Ball>(), shouldAnimate:false);
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

            Ball nextUnit = GetRandomBallUnit().GetComponent<Ball>();
            if (UnityEngine.Random.Range(0, 2) == 1) {
                var specialType = RNGSpecialType();
                if (specialType != SpecialUnit.None) nextUnit.specialType = specialType;
            }            
            
            randomTile.SetQueuedBallUnit(nextUnit);
            ballUnitQueue.Enqueue(randomTile);
        }

        HUDManager.Instance.SetQueue(ballUnitQueue);
        GameManager.Instance.ChangeState(GameState.PlayerTurn);                
    }
    
    public GameObject GetRandomBallUnit()
    {                
        Type type = typeof(BallUnitType);
        Array values = type.GetEnumValues();
        int index = UnityEngine.Random.Range(0, values.Length);
        BallUnitType ballUnitType = (BallUnitType)values.GetValue(index);
        return ObjectPooler.Instance.SpawnFromPool(ballUnitType.ToString());
    }

    public SpecialUnit RNGSpecialType()
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