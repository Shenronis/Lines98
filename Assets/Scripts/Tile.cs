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
    public bool Walkable  => OccupiedBallUnit == null;
    public bool Spawnable => queuedBallIndicator == null && OccupiedBallUnit == null;
    public bool isSelected;    
    private Vector3[] pathVectorArray;
    private Sequence floatingSeq;
    private BoxCollider2D boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (PauseMenu.isPaused) boxCollider.enabled = false;
        else boxCollider.enabled = true;
    }

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

    public void SetHighlight()
    {        
        hightlightMask.SetActive(true);
    }

    public void ClearHighlight()
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
            // Swap selection if there was a previous selection of a ball unit            
            if (BallUnitManager.Instance.selectedBallUnit ) {

                // Remove selected logic & indicator
                var ballUnit = BallUnitManager.Instance.selectedBallUnit;
                ballUnit.OccupiedTile.isSelected = false;
                ballUnit.OccupiedTile.ClearHighlight();
                ballUnit.OccupiedTile.StopFloatSelected();

                /*
                    But if the previous selection was this one too!?
                    we deselect selection and cancel swap
                */
                if (ballUnit == this.OccupiedBallUnit) {
                    BallUnitManager.Instance.selectedBallUnit = null;
                    return;
                };
            }
            
            // Set new current selected
            BallUnitManager.Instance.selectedBallUnit = OccupiedBallUnit;
            isSelected = true;
            SetHighlight();            
            FloatSelected();
        }
        else // Else if no ball unit at the selection
        {
            /*
                But if we're currently selected a ball unit
                we check path and move it
            */
            if (BallUnitManager.Instance.selectedBallUnit)
            {                                                
                var selectedUnit = BallUnitManager.Instance.selectedBallUnit;
                var selectedTile = selectedUnit.OccupiedTile;
                var destination = this.transform.position;
                selectedTile.StopFloatSelected(); // stop flying around and get moving

                // Pathfind algo returns a Vector3[]
                pathVectorArray = GridManager.Instance.findPath(selectedTile.transform.position, destination);

                /*
                    If found path
                    true => move
                    false => remove selected logic & indicator
                */
                if (pathVectorArray!=null && pathVectorArray.Length > 1)
                {                
                    //Remove starting position
                    pathVectorArray = pathVectorArray.Skip(1).ToArray();

                    //Debug                    
                    for (int i = 0; i < pathVectorArray.Length - 1; i++)
                    {
                        Debug.DrawLine(pathVectorArray[i], pathVectorArray[i+1], Color.cyan, 10f);                        
                    }
                    //~Debug

                    // Move the ball!
                    SetBallUnitPosition(selectedUnit, shouldAnimate:true);

                    // Don't forget the SFX(s)
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

            //Deselect
            BallUnitManager.Instance.selectedBallUnit = null;        
        }   
    }

    public void FloatSelected()
    {
        if (floatingSeq !=null && floatingSeq.IsPlaying()) return;

        floatingSeq = DOTween.Sequence();
        floatingSeq.SetAutoKill(false);
        floatingSeq.Append(OccupiedBallUnit.transform.DOBlendableMoveBy(new Vector3(0,0.2f), 0.5f))
            .Append(OccupiedBallUnit.transform.DOBlendableLocalMoveBy(new Vector3(0,-0.2f), 0.5f))
            .AppendInterval(0.1f)
            .OnComplete(()=>{floatingSeq.Restart();});
    }

    public void StopFloatSelected()
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
                .OnWaypointChange((int waypoint) => {
                    /*
                        Track Pacman's path and Ghost for
                        their special abilities
                    */
                    if (ballUnit.specialType == SpecialUnit.Pacman)
                    {                        
                        var pos = ballUnit.transform.position;
                        var effectedTile = GridManager.Instance.GetTileAtPos(pos);

                        if (effectedTile.OccupiedBallUnit != null && effectedTile.OccupiedBallUnit != ballUnit) {
                            var explodeFX = ObjectPooler.Instance.GetFromPool("Explosion");
                            explodeFX.transform.position = effectedTile.transform.position;
                            
                            var FXmain = explodeFX.GetComponent<ParticleSystem>().main;
                            FXmain.startColor = effectedTile.OccupiedBallUnit.GetSpriteColor();

                            explodeFX.gameObject.SetActive(true);

                            SoundEffectPlayer.Instance.Play(SFX.pop);

                            effectedTile.RemoveBall();
                        }
                    }
                    else if (ballUnit.specialType == SpecialUnit.Ghost)
                    {
                        var pos = ballUnit.transform.position;
                        var effectedTile = GridManager.Instance.GetTileAtPos(pos);
                        var unit = effectedTile.OccupiedBallUnit;

                        // How long should the cardbox be revealed by Ghost?
                        float revealDuration = 3.0f;

                        if (unit != null && unit != ballUnit) {
                            // Solid Snake?
                            if (unit.specialType == SpecialUnit.Cardbox) {
                                var unitMask = unit.GetMask();
                                if (unitMask.color.a != 1f) return;

                                Sequence fadeSequence = DOTween.Sequence();
                                fadeSequence.Append(unitMask.DOFade(0.3f,0.5f))
                                    .AppendInterval(revealDuration)
                                    .Append(unitMask.DOFade(1f, 0.5f));
                            }                                
                        }
                    }
                })
                .OnComplete(() => {
                    // Check lines
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
    
    public void Explodes(List<Tile> connectedTiles, bool byPlayer=true)
    {        
        // Pooof!
        HashSet<Tile> tiles = new HashSet<Tile>();
        bool containUnit = false;
        bool containBomb = false;

        foreach(var tile in connectedTiles)
        {
            if (tile.OccupiedBallUnit.specialType == SpecialUnit.Bomb)
            {                
                /*
                    Find AreaOfEffect to set VFX
                    by, again, get neighbors around bomb :(                    
                */
                containBomb = true;
                List<GameObject> AoE = new List<GameObject>();
                var position =  tile.transform.position;
                var explodeFX = ObjectPooler.Instance.GetFromPool("BombExplosion");
                explodeFX.transform.position = position;
                AoE.Add(explodeFX);

                // find neighbors in AoE to bomb, the units' neighbor, i meant
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x==0 && y==0) continue;

                        if (GridManager.Instance.IsValidPosition(Mathf.RoundToInt(position.x) + x, Mathf.RoundToInt(position.y) + y))
                        {
                            var neighborTile = GridManager.Instance.GetTileAtPos(position.x + x, position.y + y);                            
                            if (neighborTile != null) {
                                explodeFX = ObjectPooler.Instance.GetFromPool("BombExplosion");
                                explodeFX.transform.position = neighborTile.transform.position;
                                AoE.Add(explodeFX);
                            }
                        }
                    }
                }

                // play VFX
                foreach(GameObject VFX in AoE)
                {
                    VFX.SetActive(true);
                }                
            }
            else
            {
                containUnit = true;

                var explodeFX = ObjectPooler.Instance.GetFromPool("Explosion");
                explodeFX.transform.position = tile.transform.position;
                
                var FXmain = explodeFX.GetComponent<ParticleSystem>().main;
                FXmain.startColor = tile.OccupiedBallUnit.GetSpriteColor();

                explodeFX.gameObject.SetActive(true);                
            }            

            // Remove Unit
            if (tile.OccupiedBallUnit != null) tile.RemoveBall();                        
        }

        if (containUnit) SoundEffectPlayer.Instance.Play(SFX.pop);
        if (containBomb) {
            CameraShake.Instance.Shake();
            SoundEffectPlayer.Instance.Play(SFX.explosion);
        }    

        GridManager.Instance.UpdatePathdinding();
        GameManager.Instance.Score += connectedTiles.Count;
        if (GameManager.Instance.gameState == GameState.PlayerTurn && byPlayer) GameManager.Instance.ChangeState(GameState.SpawnAndQueue);
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
        if (connectedTiles != null) Explodes(connectedTiles, byPlayer:false);
        else GridManager.Instance.UpdatePathdinding();
    }

    private void RemoveQueuedBall()
    {
        queuedBallIndicator.transform.localScale = BallUnitSize.NORMAL;        
        queuedBallIndicator.gameObject.SetActive(false);
        queuedBallIndicator = null;
    }

    private void RemoveBall()
    {
        OccupiedBallUnit.OccupiedTile = null;
        OccupiedBallUnit.gameObject.SetActive(false);
        OccupiedBallUnit = null;
    }

    private void RemoveFromLastSelectedTile(Ball ballUnit)
    {
        ballUnit.OccupiedTile.isSelected = false;
        ballUnit.OccupiedTile.ClearHighlight();
        ballUnit.OccupiedTile.OccupiedBallUnit = null; 
    }

    public void RefreshTile()
    {
        if (OccupiedBallUnit != null) RemoveBall();
        if (queuedBallIndicator != null) RemoveQueuedBall();
        
        isSelected = false;
        ClearHighlight();
        StopFloatSelected();
        GridManager.Instance.UpdatePathdinding();
    }

    public void SelfDestruct()
    {
        var position =  this.transform.position;
        var explodeFX = ObjectPooler.Instance.GetFromPool("BombExplosion");        
        explodeFX.transform.position = position;

        RefreshTile();
        explodeFX.SetActive(true);
    }
}
