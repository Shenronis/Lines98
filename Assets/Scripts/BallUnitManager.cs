using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallUnitManager : MonoBehaviour
{
    #region Singleton
    public static BallUnitManager Instance {get; private set;}
    void Awake()
    {
        Instance = this;
        ballUnitQueue = new Queue<Tile>();
    }
    #endregion

    public Ball selectedBallUnit;
    Queue<Tile> ballUnitQueue;

    public void SpawnBallUnits(int count = 3)
    {
        for (int i = 0; i < count; i++)
        {            
            if ((ballUnitQueue != null) && (ballUnitQueue.Count >= 1)) {
                var nextTile = ballUnitQueue.Dequeue();
                if (nextTile.OccupiedBallUnit)
                {
                    nextTile.queuedBallIndicator = null;
                }
                else
                {
                    nextTile.SetQueueToBall();
                }
            } else {                                
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
        for (int i = 0; i < count; i++)
        {
            Tile randomTile = GridManager.Instance.GetRandomTilePosition();
            if (randomTile == null) { return; }

            Ball nextUnit = GetRandomBallUnit().GetComponent<Ball>();
            randomTile.SetQueuedBallUnit(nextUnit);

            ballUnitQueue.Enqueue(randomTile);
        }

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
}