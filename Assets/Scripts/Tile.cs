using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject hightlightMask;
    public Ball OccupiedBallUnit;
    public Ball queuedBallIndicator;
    public bool Walkable => OccupiedBallUnit == null;
    public bool Spawnable => queuedBallIndicator == null && OccupiedBallUnit == null;
    public bool isSelected;
    private Vector3[] pathVectorArray;
    
    void OnMouseEnter()
    {                
        if (GameManager.Instance.gameState == GameState.PlayerTurn) {            
            SetHighlight();
        }        
    }

    void OnMouseExit()
    {
        if (GameManager.Instance.gameState == GameState.PlayerTurn) {
            ClearHighlight();
        } 
    }

    void SetHighlight()
    {        
        hightlightMask.SetActive(true);
    }

    void ClearHighlight()
    {        
        if (isSelected) return;
        hightlightMask.SetActive(false);
    }

    void OnMouseDown()
    {
        // If not player's turn, ignore
        if (GameManager.Instance.gameState != GameState.PlayerTurn) { return; }

        // If selected cell have a ball unit
        if (OccupiedBallUnit != null)
        {
            // If there was a previous selection of a ball unit
            // then remove selected indicator
            if (BallUnitManager.Instance.selectedBallUnit && BallUnitManager.Instance.selectedBallUnit.OccupiedTile != this) {                
                var ballUnit = BallUnitManager.Instance.selectedBallUnit;
                ballUnit.OccupiedTile.isSelected = false;
                ballUnit.OccupiedTile.ClearHighlight();                
            }
            
            BallUnitManager.Instance.selectedBallUnit = OccupiedBallUnit;
            isSelected = true;
            SetHighlight();         
        }
        else // Else if no ball unit at the selection
        {
            // But then if we're currently having a selected ball unit
            // we check path and move it
            if (BallUnitManager.Instance.selectedBallUnit)
            {                                                
                var selectedUnit = BallUnitManager.Instance.selectedBallUnit;
                var destination = this.transform.position;
                pathVectorArray = GridManager.Instance.findPath(selectedUnit.transform.position, destination);

                // If found path
                if (pathVectorArray!=null && pathVectorArray.Length > 1)
                {                
                    //Remove start position
                    pathVectorArray = pathVectorArray.Skip(1).ToArray();

                    //Debug                    
                    for (int i = 0; i < pathVectorArray.Length - 1; i++)
                    {
                        Debug.DrawLine(pathVectorArray[i], pathVectorArray[i+1], Color.cyan, 10f);                        
                    }
                    //~Debug

                    SetBallUnitPosition(selectedUnit, shouldAnimate:true);                    
                }
                else
                {
                    var ballUnit = BallUnitManager.Instance.selectedBallUnit;
                    ballUnit.OccupiedTile.isSelected = false;
                    ballUnit.OccupiedTile.ClearHighlight();  
                }                
            }
            BallUnitManager.Instance.selectedBallUnit = null;        
        }   
    }

    public void SetBallUnitPosition(Ball ballUnit, bool shouldAnimate = false)
    {                
        if (ballUnit.OccupiedTile != null) RemoveFromLastSelectedTile(ballUnit);        
        if (this.queuedBallIndicator != null) RemoveQueuedBall();
        this.OccupiedBallUnit = ballUnit;
        ballUnit.OccupiedTile = this;

        GridManager.Instance.UpdatePathdinding();        

        if (shouldAnimate)
        {
            ballUnit.transform.DOPath(pathVectorArray, 0.5f)
                .OnComplete(() => {
                    // Check line
                    List<Tile> connectedTiles = GridManager.Instance.checkLines(this.transform.position);
                    if (connectedTiles != null && connectedTiles.Count > 0) Explodes(connectedTiles);
                    else GameManager.Instance.ChangeState(GameState.SpawnAndQueue);
                });
        }
        else
        {
            ballUnit.transform.position = this.transform.position;
        }
    }

    public void Explodes(List<Tile> connectedTiles)
    {
        foreach(var tile in connectedTiles)
        {
            // Pooof!
            tile.OccupiedBallUnit.gameObject.SetActive(false);
            tile.OccupiedBallUnit = null;            
        }

        GameManager.Instance.Score += connectedTiles.Count;

        GameManager.Instance.ChangeState(GameState.SpawnAndQueue);
    }

    public void SetQueuedBallUnit(Ball ballUnit, bool shouldAnimate = false)
    {                    
        this.queuedBallIndicator = ballUnit;
        ballUnit.transform.position = this.transform.position;
        ballUnit.transform.localScale = Vector3.zero;
        ballUnit.transform.DOScale(BallUnitSize.QUEUE, 0.5f);     
    }

    public void SetQueueToBall()
    {                        
        this.queuedBallIndicator.transform.DOScale(BallUnitSize.NORMAL, 0.5f);
        this.OccupiedBallUnit = this.queuedBallIndicator;
        this.OccupiedBallUnit.OccupiedTile = this;
        GridManager.Instance.UpdatePathdinding();
    }

    private void RemoveQueuedBall()
    {
        this.queuedBallIndicator.transform.localScale = BallUnitSize.NORMAL;
        this.queuedBallIndicator.gameObject.SetActive(false);
        this.queuedBallIndicator = null;
    }

    private void RemoveFromLastSelectedTile(Ball ballUnit)
    {
        ballUnit.OccupiedTile.isSelected = false;
        ballUnit.OccupiedTile.ClearHighlight();
        ballUnit.OccupiedTile.OccupiedBallUnit = null; 
    }
}
