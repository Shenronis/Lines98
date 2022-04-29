using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region Singleton
    public static GridManager Instance {get; private set;}
    void Awake()
    {
        Instance = this;
    }
    #endregion

    [SerializeField] private int width, height;
    [SerializeField] private Tile TilePrefab;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private LineRenderer pathVisual;
    private Dictionary<Vector2Int, Tile> cells;
    private Pathfinding pathfinder;
    private Vector3[] path;

    public void Draw(Vector2 pos)
    {
        if (BallUnitManager.Instance.selectedBallUnit != null) {
            var selectedBallUnit = BallUnitManager.Instance.selectedBallUnit;
            var selectedTile = selectedBallUnit.OccupiedTile;
            var hoveredTile = GetTileAtPos(pos);
            path = pathfinder.findPath(selectedTile.transform.position, hoveredTile.transform.position);

            for (int i = 0; i < path.Length-1; i++)
            {
                Debug.DrawLine(path[i], path[i+1], Color.red, 1f);
            }

            pathVisual.GetComponent<LineRenderController>().SetUpLine(path);
        }
    }

    public void Erase()
    {
        pathVisual.GetComponent<LineRenderController>().RemoveLine();
    }

    public Dictionary<Vector2Int, Tile> GetGrid()
    {
        return cells;
    }

    public void GenerateGrid(bool restart=false)
    {
        if (restart) {
            // Restart all object's state
            RestartAllTiles();
            GameManager.Instance.Score = 0;
            Timer.Instance.SetTime(0);
            BallUnitManager.Instance.ClearQueue();
            GameManager.Instance.ChangeState(GameState.SpawnAndQueue);            
            return;
        }         

        cells = new Dictionary<Vector2Int, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnTile = Instantiate(TilePrefab, new Vector3(x,y), Quaternion.identity);
                spawnTile.name = $"Tile {x}-{y}";

                // Grab Tile reference
                cells[new Vector2Int(x,y)] = spawnTile;
            }
        }

        // Camera stuffs
        mainCamera.position = new Vector3((float)width * 0.85f - 0.5f, (float)height/2 - 0.5f, -10f);        
        CameraShake.Instance.SetCameraOrigin(mainCamera.position);

        // Initiate pathfind on Grid(W x H)
        pathfinder = new Pathfinding(cells, width, height);

        GameManager.Instance.ChangeState(GameState.SpawnAndQueue);  
    }

    public Tile GetTileAtPos(Vector2 pos)
    {
        var x = Mathf.RoundToInt(pos.x);
        var y = Mathf.RoundToInt(pos.y);
        if(cells.TryGetValue(new Vector2Int(x,y), out var tile)) {
            return tile;
        }

        return null;
    }

    public Tile GetTileAtPos(float x, float y)
    {
        return GetTileAtPos(new Vector2(x,y));
    }

    /// <summary>
    /// Grab a random Tile.spawnable==true from grid
    /// </summary>
    /// <returns>Tile</returns>
    public Tile GetRandomSpawnableTilePosition()
    {        
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.Spawnable).OrderBy(t => Random.value).First().Value;
        return tile;
    }

    /// <summary>
    /// Return number of Tile.spawnable
    /// </summary>
    /// <returns>int</returns>
    public int CountSpawnableTile()
    {
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.Spawnable);
        return tile.Count();
    }

    /// <summary>
    /// Return numer of Tile.OccupiedBallUnit==null
    /// </summary>
    /// <returns>int</returns>
    public int CountHasNotOccupiedTile()
    {
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.OccupiedBallUnit == null);
        return tile.Count();
    }

    /// <summary>
    /// Return number of Tile.Walkable==true
    /// </summary>
    /// <returns>int</returns>
    public int CountMoveableTile()
    {
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.Walkable);
        return tile.Count();
    }

    /// <summary>
    /// Update pathfind's matrix
    /// </summary>
    public void UpdatePathdinding()
    {
        pathfinder.UpdatePathdinding();
    }

    /// <summary>
    /// Call pathfind's algo
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">End position</param>
    /// <returns>Vector3[]</returns>
    public Vector3[] findPath(Vector2 start, Vector2 end)
    {
        return pathfinder.findPath(start, end);
    }

    /// <summary>
    /// Return a list of continously balls with same color
    /// checking from the pivot's position
    /// </summary>
    /// <param name="pivot">Pivot's position</param>
    /// <returns>List<Tile></returns>
    public List<Tile> checkLines(Vector2 pivot)
    {                
        HashSet<Tile> connectedTiles = new HashSet<Tile>(); // Usage: handles overlapping bombs AoE
        int[] u = new int[] { 0, 1, 1, 1 };
        int[] v = new int[] { 1, 0, -1, 1 };
        int x,y,colorMatched;
        int _x = Mathf.RoundToInt(pivot.x);
        int _y = Mathf.RoundToInt(pivot.y);
        Tile pivotTile = GetTileAtPos(new Vector2(_x,_y));        

        // Continous line check
        for (int direction = 0; direction < 4; direction++)
        {
            colorMatched = 0;
            x = _x;
            y = _y;

            while (true)
            {                
                x += u[direction];
                y += v[direction];                
                if (!IsValidPosition(x,y)) break;

                var tile = GetTileAtPos(new Vector2(x, y));
                if (!IsValidTile(pivotTile, tile)) break;

                colorMatched++;
            }

            x = _x;
            y = _y;

            while (true)
            {
                x -= u[direction];
                y -= v[direction];                
                if (!IsValidPosition(x,y)) break;

                var tile = GetTileAtPos(new Vector2(x, y));
                if (!IsValidTile(pivotTile, tile)) break;

                colorMatched++;
            }
            
            // Includes itself!
            colorMatched++;
            
            if (colorMatched >= 5)
            {
                while (colorMatched-- > 0)
                {
                    x += u[direction];
                    y += v[direction];

                    if (x != _x || y != _y)
                    {
                        connectedTiles.Add(GetTileAtPos(new Vector2(x,y)));
                    }
                }
            }
        }
    
        if (connectedTiles.Count > 0) connectedTiles.Add(pivotTile);
        else return null;


        /*
            This part is to include every other unit is in the bomb's AoE (a 3x3 matrix)
            and check if it also trigger the other bomb(s) in AoE, hence chaining the bombs
        */
        bool minesweeper; // should loop to include the chained bomb's AoE
        do
        {
            minesweeper = false;
            foreach(Tile tile in connectedTiles.ToList())
            {
                /*
                    As it will explode all surrounding ball unit (by 3x3 matrix)
                    we need to find all the neighbors 
                    and exclude ones we already set for explode
                */
                if (tile.OccupiedBallUnit.specialType == BallUnitSpecial.Bomb)
                {
                    var position =  tile.transform.position;
                    List<Tile> neighbors = new List<Tile>();
                                    
                    for (int xCoord = -1; xCoord <= 1; xCoord++)
                    {
                        for (int yCoord = -1; yCoord <= 1; yCoord++)
                        {
                            // Exclude itself
                            if (xCoord==0 && yCoord==0) continue;

                            if (IsValidPosition(Mathf.RoundToInt(position.x) + xCoord, Mathf.RoundToInt(position.y) + yCoord))
                            {
                                var neighborTile = GetTileAtPos(position.x + xCoord, position.y + yCoord);

                                // Check for overlap
                                if (connectedTiles.Contains(neighborTile)) continue;

                                if (neighborTile.OccupiedBallUnit != null)
                                {
                                    // Check if another bomb was triggered
                                    if (neighborTile.OccupiedBallUnit.specialType == BallUnitSpecial.Bomb) {minesweeper=true;}
                                    neighbors.Add(neighborTile);
                                }
                            }
                        }
                    }
                                    
                    connectedTiles.UnionWith(neighbors);
                }
            }
        } while (minesweeper);

        return connectedTiles.ToList();
    }


    private bool IsValidTile(Tile pivotTile, Tile tile)
    {
        if (tile.OccupiedBallUnit == null) return false;

        // Crate can only be destroyed by bomb
        if (tile.OccupiedBallUnit.specialType == BallUnitSpecial.Crate) return false;

        if (pivotTile.OccupiedBallUnit.Type != tile.OccupiedBallUnit.Type) return false;

        return true;
    }

    /// <summary>
    /// Check if the given coordinates is within grid
    /// </summary>
    /// <param name="x">Position's x</param>
    /// <param name="y">Position's y</param>
    /// <returns>bool</returns>
    public bool IsValidPosition(int x, int y)
    {
        if (x < 0 || y < 0) return false;
        if (x >= width || y >= height) return false;
        return true;
    }

    /// <summary>
    /// Run Tile.RefreshTile() on all Tile in grid
    /// </summary>
    public void RestartAllTiles()
    {
        foreach(KeyValuePair<Vector2Int, Tile> cell in cells)
        {
            cell.Value.RefreshTile();
        }
    }



    /*
        A little hidden input
        
        BIGBANG

        a Cheat Code in GTA Vice City
        for blowing up vehicle
    */
    private KeyCode[] nukeSequence = new KeyCode[]
    {
        KeyCode.B,
        KeyCode.I,
        KeyCode.G,
        KeyCode.B,
        KeyCode.A,
        KeyCode.N,
        KeyCode.G,        
    };
    private int sequenceIndex;
    bool hasFired = false;
    
    private void Update() {

        /*
            Cheat code that will demolish the game board
            and can only be used one per game
        */
        if (Input.GetKeyDown(nukeSequence[sequenceIndex]) && !hasFired) {
            CameraShake.Instance.Shake(0.3f, 0.2f);
            SoundEffectPlayer.Instance.Play(SFX.select);

            if (++sequenceIndex == nukeSequence.Length){
                sequenceIndex = 0;
                hasFired = true;
                TACTICALNUKE();                                
            }

        } else if (Input.anyKeyDown) sequenceIndex = 0;
    }

    public void TACTICALNUKE()
    {
        foreach(KeyValuePair<Vector2Int, Tile> cell in cells)
        {
            cell.Value.SelfDestruct();
        }
        SoundEffectPlayer.Instance.Play(SFX.explosion);
        GenerateGrid(restart:true);
    }
}