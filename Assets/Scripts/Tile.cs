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

            // Visual movement indicator
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

    /// <summary>
    /// Handles selecting operation
    /// </summary>
    void OnMouseDown()
    {
        // If not player's turn, ignore
        if (GameManager.Instance.gameState != GameState.PlayerTurn) { return; }

        // Remove visual movment indicator
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
                    if (selectedUnit.specialType == BallUnitSpecial.Ghost) SoundEffectPlayer.Instance.Play(SFX.ghost);
                    if (selectedUnit.specialType == BallUnitSpecial.Bomb) SoundEffectPlayer.Instance.Play(SFX.bomb);
                    if (selectedUnit.specialType == BallUnitSpecial.Pacman) SoundEffectPlayer.Instance.Play(SFX.pacman);
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

    /// <summary>
    /// Float the occupied ball unit with DOTween.Sequence
    /// </summary>
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

    /// <summary>
    /// Kill the ball unit's floating DOTween.Sequence
    /// </summary>
    public void StopFloatSelected()
    {
        if (floatingSeq != null && floatingSeq.IsPlaying())
        {
            floatingSeq.OnComplete(null);
            floatingSeq.Kill(complete:true);
            floatingSeq = null;
        }
    }

    /// <summary>
    /// Main operation to move the ball after selecting the unit and an empty tile
    /// </summary>
    /// <param name="ballUnit">Unit to move</param>
    /// <param name="shouldAnimate">FALSE to simply place the ball there</param>
    public void SetBallUnitPosition(Ball ballUnit, bool shouldAnimate = false)
    {                            
        if (ballUnit.OccupiedTile != null) RemoveFromLastSelectedTile(ballUnit);        
        if (this.queuedBallIndicator != null) RemoveQueuedBall(); //Block the queue ball unit (seems like this core behavior was missing in the requirement)
        this.OccupiedBallUnit = ballUnit;
        ballUnit.OccupiedTile = this;
        
        GridManager.Instance.UpdatePathdinding();        

        if (shouldAnimate)
        {            
            ballUnit.transform.DOPath(pathVectorArray, 0.5f)
                // This will trigger everytime DOPath switches to a new point in Vector3[]pathVectorArray
                .OnWaypointChange((int waypoint) => {   
                    /*
                        Track Pacman's path and Ghost for
                        their special abilities
                    */
                    if (ballUnit.specialType == BallUnitSpecial.Pacman)
                    {                        
                        var pos = ballUnit.transform.position;
                        var effectedTile = GridManager.Instance.GetTileAtPos(pos);
                    
                        // Eats every unit on it's way, but don't contribute to points
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
                    else if (ballUnit.specialType == BallUnitSpecial.Ghost)
                    {
                        var pos = ballUnit.transform.position;
                        var effectedTile = GridManager.Instance.GetTileAtPos(pos);
                        var unit = effectedTile.OccupiedBallUnit;

                        // How long should the cardbox unit be revealed by Ghost?
                        float revealDuration = 3.0f;

                        if (unit != null && unit != ballUnit) {
                            // Check for Solid Snake?
                            if (unit.specialType == BallUnitSpecial.Cardbox) {
                                var unitMask = unit.GetMask();

                                // if is unit is being revealed then do nothing
                                if (unitMask.color.a != 1f) return; 

                                // Fading the mask DOTween.Sequence
                                Sequence fadeSequence = DOTween.Sequence();
                                fadeSequence.Append(unitMask.DOFade(0.3f,0.5f))
                                    .AppendInterval(revealDuration)
                                    .Append(unitMask.DOFade(1f, 0.5f));
                            }                                
                        }
                    }
                })
                .OnComplete(() => {
                    /*
                        Check existing continuous line
                        else end turn
                    */
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
    
    /// <summary>
    /// Pooof!
    /// This handle the ball unit explode/pop behavior and calculate score
    /// 
    /// Also checking if this was triggered by Player to end the turn
    /// because if there were (a) previous queued ball unit(s) set into tile
    /// then it will also become a valid continous line, thus triggering this method        
    /// </summary>
    /// 
    /// <param name="connectedTiles">The continous? line to destroy</param>
    /// <param name="byPlayer">If was trigger in player's turn</param>
    public void Explodes(List<Tile> connectedTiles, bool byPlayer=true)
    {                        
        bool containUnit = false;
        bool containBomb = false;

        foreach(var tile in connectedTiles)
        {
            if (tile.OccupiedBallUnit.specialType == BallUnitSpecial.Bomb)
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
                explodeFX.SetActive(true);

                // find neighbors in AoE to bomb, the units' neighbor(s), i meant
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
                                explodeFX.SetActive(true);
                            }
                        }
                    }
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

        // Play SFX
        if (containUnit) SoundEffectPlayer.Instance.Play(SFX.pop);
        if (containBomb) {
            CameraShake.Instance.Shake();
            SoundEffectPlayer.Instance.Play(SFX.explosion);
        }    

        GridManager.Instance.UpdatePathdinding();
        
        // Update score based on how many unit was exploded/popepd
        GameManager.Instance.Score += connectedTiles.Count;

        if (GameManager.Instance.gameState == GameState.PlayerTurn && byPlayer) GameManager.Instance.ChangeState(GameState.SpawnAndQueue);
    }

    /// <summary>
    /// Set ball unit as queue state (indicator)
    /// </summary>
    /// <param name="ballUnit">Ball</param>
    /// <param name="shouldAnimate">FALSE to simply place it there</param>
    public void SetQueuedBallUnit(Ball ballUnit, bool shouldAnimate = false)
    {             
        if (this.OccupiedBallUnit) {
            RemoveQueuedBall();
            return;
        }

        this.queuedBallIndicator = ballUnit;
        ballUnit.transform.position = this.transform.position;
        ballUnit.transform.localScale = Vector3.zero;           // Pre-set for animation
        ballUnit.transform.DOScale(BallUnitSize.QUEUE, 0.5f);   // Grow into smol size
    }

    /// <summary>
    /// Set the queued ball (indicator) to fully appear, thus occupying the tile
    /// </summary>
    public void SetQueueToBall()
    {                   
        if (this.queuedBallIndicator == null) return;
        if (this.OccupiedBallUnit) {
            RemoveQueuedBall();
            return;
        }

        this.queuedBallIndicator.transform.DOScale(BallUnitSize.NORMAL, 0.5f);  // Fully grown animation


        this.OccupiedBallUnit = this.queuedBallIndicator;        
        this.OccupiedBallUnit.OccupiedTile = this;
        this.queuedBallIndicator = null;
        
        // Check if this freshly appeared unit trigger a continous line
        List<Tile> connectedTiles = GridManager.Instance.checkLines(this.transform.position);
        if (connectedTiles != null) Explodes(connectedTiles, byPlayer:false);
        else GridManager.Instance.UpdatePathdinding();
    }

    /// <summary>
    /// Remove the queue ball (indicator)
    /// </summary>
    private void RemoveQueuedBall()
    {
        queuedBallIndicator.transform.localScale = BallUnitSize.NORMAL;        
        queuedBallIndicator.gameObject.SetActive(false);
        queuedBallIndicator = null;
    }

    /// <summary>
    /// Remove the ball unit
    /// </summary>
    private void RemoveBall()
    {
        OccupiedBallUnit.OccupiedTile = null;
        OccupiedBallUnit.gameObject.SetActive(false);
        OccupiedBallUnit = null;
    }

    /// <summary>
    /// Remove the ball unit we just move from the previous tile
    /// </summary>
    /// <param name="ballUnit"></param>
    private void RemoveFromLastSelectedTile(Ball ballUnit)
    {
        ballUnit.OccupiedTile.isSelected = false;
        ballUnit.OccupiedTile.ClearHighlight();
        ballUnit.OccupiedTile.OccupiedBallUnit = null; 
    }

    /// <summary>
    /// Restore Tile as new (empty)
    /// </summary>
    public void RefreshTile()
    {
        if (OccupiedBallUnit != null) RemoveBall();
        if (queuedBallIndicator != null) RemoveQueuedBall();
        
        isSelected = false;
        ClearHighlight();
        StopFloatSelected();
        GridManager.Instance.UpdatePathdinding();
    }

    /// <summary>
    /// RefreshTile(), but we're bombing the Tile
    /// </summary>
    public void SelfDestruct()
    {
        var position =  this.transform.position;
        var explodeFX = ObjectPooler.Instance.GetFromPool("BombExplosion");        
        explodeFX.transform.position = position;

        RefreshTile();
        explodeFX.SetActive(true);
    }
}