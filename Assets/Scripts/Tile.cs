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
    private Sequence floatingSeq;
    private Tween scaleTween;

    void OnMouseEnter()
    {                
        if (GameManager.Instance.gameState == GameState.PlayerTurn) {

            if (!OccupiedBallUnit) GridManager.Instance.Draw(this.transform.position);
            else GridManager.Instance.Erase();

            SetHighlight();
        }
    }

    void OnMouseExit()
    {
        if (GameManager.Instance.gameState == GameState.PlayerTurn) ClearHighlight();        
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

        // Remove pathfinding visual
        GridManager.Instance.Erase();

        // If selected cell have a ball unit
        if (OccupiedBallUnit != null)
        {
            // If there was a previous selection of a ball unit
            // then remove selected indicator
            if (BallUnitManager.Instance.selectedBallUnit && BallUnitManager.Instance.selectedBallUnit.OccupiedTile != this) {                
                var ballUnit = BallUnitManager.Instance.selectedBallUnit;
                ballUnit.OccupiedTile.isSelected = false;
                ballUnit.OccupiedTile.ClearHighlight();
                ballUnit.OccupiedTile.StopFloatSelected();
            }
            
            BallUnitManager.Instance.selectedBallUnit = OccupiedBallUnit;
            isSelected = true;
            SetHighlight();
            FloatSelected();
        }
        else // Else if no ball unit at the selection
        {
            // But then if we're currently having a selected ball unit
            // we check path and move it
            if (BallUnitManager.Instance.selectedBallUnit)
            {                                                
                var selectedUnit = BallUnitManager.Instance.selectedBallUnit;
                var selectedTile = selectedUnit.OccupiedTile;
                var destination = this.transform.position;
                selectedTile.StopFloatSelected();

                pathVectorArray = GridManager.Instance.findPath(selectedTile.transform.position, destination);

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

                    // Move the ball!
                    SetBallUnitPosition(selectedUnit, shouldAnimate:true);

                    // Don't forget SFX
                    if (selectedUnit.specialType == SpecialUnit.Ghost) SoundEffectPlayer.Instance.Play(SFX.ghost);
                    if (selectedUnit.specialType == SpecialUnit.Bomb) SoundEffectPlayer.Instance.Play(SFX.bomb);
                    SoundEffectPlayer.Instance.Play(SFX.move);
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

    void FloatSelected()
    {
        if (floatingSeq !=null && floatingSeq.IsPlaying()) return;

        floatingSeq = DOTween.Sequence();
        floatingSeq.SetAutoKill(false);
        floatingSeq.Append(OccupiedBallUnit.transform.DOBlendableMoveBy(new Vector3(0,0.2f), 0.5f))
            .Append(OccupiedBallUnit.transform.DOBlendableLocalMoveBy(new Vector3(0,-0.2f), 0.5f))
            .AppendInterval(0.5f)
            .OnComplete(()=>{floatingSeq.Restart();});
    }

    void StopFloatSelected()
    {
        if (floatingSeq != null && floatingSeq.IsPlaying())
        {
            floatingSeq.OnComplete(null);
            floatingSeq.Kill(complete:true);
            floatingSeq = null;
        }
    }

    public void SetBallUnitPosition(Ball ballUnit, bool shouldAnimate = false)
    {                    
        if (ballUnit.OccupiedTile != null) RemoveFromLastSelectedTile(ballUnit);        
        if (this.queuedBallIndicator != null) RemoveQueuedBall();
        this.OccupiedBallUnit = ballUnit;
        ballUnit.OccupiedTile = this;

        GridManager.Instance.UpdatePathdinding();

        //debug
        // List<Tile> connectedTiles = GridManager.Instance.checkLines(this.transform.position);
        // if (connectedTiles != null) Explodes(connectedTiles);
        // return;
        //debug

        if (shouldAnimate)
        {
            ballUnit.transform.DOPath(pathVectorArray, 0.5f)
                .OnComplete(() => {
                    // Check line
                    List<Tile> connectedTiles = GridManager.Instance.checkLines(this.transform.position);
                    if (connectedTiles != null) Explodes(connectedTiles);
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
        // Pooof!
        foreach(var tile in connectedTiles)
        {
            if (tile.OccupiedBallUnit.specialType == SpecialUnit.Bomb)
            {                
                // Find AreaOfEffect to set VFX
                // by, again, get neighbors around bomb
                // :(                                
                List<GameObject> AoE = new List<GameObject>();
                var position =  tile.transform.position;
                var explodeFX = ObjectPooler.Instance.GetFromPool("BombExplosion");
                explodeFX.transform.position = position;
                AoE.Add(explodeFX);

                #region find neighbors in AoE to bomb, allahu akbar
                for (int xx = -1; xx <= 1; xx++)
                {
                    for (int yy = -1; yy <= 1; yy++)
                    {
                        if (xx==0 && yy==0) continue;

                        if (GridManager.Instance.IsValidPosition(Mathf.RoundToInt(position.x) + xx, Mathf.RoundToInt(position.y) + yy))
                        {
                            var neighborTile = GridManager.Instance.GetTileAtPos(position.x + xx, position.y + yy);                            
                            if (neighborTile != null) {
                                explodeFX = ObjectPooler.Instance.GetFromPool("BombExplosion");
                                explodeFX.transform.position = neighborTile.transform.position;
                                AoE.Add(explodeFX);
                            }
                        }
                    }
                }
                #endregion

                foreach(GameObject VFX in AoE)
                {
                    VFX.SetActive(true);
                }

                SoundEffectPlayer.Instance.Play(SFX.explosion);
            }
            else
            {
                var explodeFX = ObjectPooler.Instance.GetFromPool("Explosion");
                explodeFX.transform.position = tile.transform.position;
                
                var FXmain = explodeFX.GetComponent<ParticleSystem>().main;
                FXmain.startColor = tile.OccupiedBallUnit.GetSpriteColor();

                explodeFX.gameObject.SetActive(true);

                SoundEffectPlayer.Instance.Play(SFX.pop);
            }            

            tile.OccupiedBallUnit.gameObject.SetActive(false);
            tile.OccupiedBallUnit = null;            
            if (tile.queuedBallIndicator != null) tile.RemoveQueuedBall();
        }

        GridManager.Instance.UpdatePathdinding();
        GameManager.Instance.Score += connectedTiles.Count;
        if (GameManager.Instance.gameState == GameState.PlayerTurn) GameManager.Instance.ChangeState(GameState.SpawnAndQueue);
    }

    public void SetQueuedBallUnit(Ball ballUnit, bool shouldAnimate = false)
    {             
        if (this.OccupiedBallUnit) {
            RemoveQueuedBall();
            return;
        }

        this.queuedBallIndicator = ballUnit;
        ballUnit.transform.position = this.transform.position;
        ballUnit.transform.localScale = Vector3.zero;
        ballUnit.transform.DOScale(BallUnitSize.QUEUE, 0.5f);     
    }

    public void SetQueueToBall()
    {                   
        if (this.queuedBallIndicator == null) return;
        if (this.OccupiedBallUnit) {
            RemoveQueuedBall();
            return;
        }

        this.queuedBallIndicator.transform.DOScale(BallUnitSize.NORMAL, 0.5f);
        this.OccupiedBallUnit = this.queuedBallIndicator;        
        this.OccupiedBallUnit.OccupiedTile = this;
        this.queuedBallIndicator = null;
        
        List<Tile> connectedTiles = GridManager.Instance.checkLines(this.transform.position);
        if (connectedTiles != null) Explodes(connectedTiles);
        else GridManager.Instance.UpdatePathdinding();
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

    public void RestartTile()
    {
        if (OccupiedBallUnit != null)
        {
            OccupiedBallUnit.OccupiedTile = null;
            OccupiedBallUnit.gameObject.SetActive(false);
        }

        if (queuedBallIndicator != null)
        {
            RemoveQueuedBall();
        }

        isSelected = false;
        ClearHighlight();
        GridManager.Instance.UpdatePathdinding();
    }
}
