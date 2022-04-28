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

    public void GenerateGrid()
    {
        cells = new Dictionary<Vector2Int, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnTile = Instantiate(TilePrefab, new Vector3(x,y), Quaternion.identity);
                spawnTile.name = $"Tile {x}-{y}";

                cells[new Vector2Int(x,y)] = spawnTile;
            }
        }

        mainCamera.position = new Vector3((float)width * 0.85f - 0.5f, (float)height/2 - 0.5f, -10f);        
        CameraShake.Instance.SetCameraOrigin(mainCamera.position);

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

    public Tile GetRandomTilePosition()
    {        
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.Spawnable).OrderBy(t => Random.value).First().Value;
        return tile;
    }

    public int CountAvailableTiles()
    {
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.Spawnable);
        return tile.Count();
    }

    public void UpdatePathdinding()
    {
        pathfinder.UpdatePathdinding();
    }

    public Vector3[] findPath(Vector2 start, Vector2 end)
    {
        return pathfinder.findPath(start, end);
    }

    public List<Tile> checkLines(Vector2 pivot)
    {                
        HashSet<Tile> connectedTiles = new HashSet<Tile>();
        int[] u = new int[] { 0, 1, 1, 1 };
        int[] v = new int[] { 1, 0, -1, 1 };
        int x,y,colorMatched;
        int _x = Mathf.RoundToInt(pivot.x);
        int _y = Mathf.RoundToInt(pivot.y);
        Tile pivotTile = GetTileAtPos(new Vector2(_x,_y));        

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

        bool minesweeper;
        do
        {
            minesweeper = false;            
            foreach(Tile tile in connectedTiles.ToList())
            {
                // Handle Bomb special ball unit
                // it will explode all surrounding ball unit (by 3x3 matrix)
                // we need to find all the neighbors 
                // and exclude ones we already set for explode
                if (tile.OccupiedBallUnit.specialType == SpecialUnit.Bomb)
                {
                    var position =  tile.transform.position;
                    List<Tile> neighbors = new List<Tile>();
                
                    #region Find Neighbors
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        for (int yy = -1; yy <= 1; yy++)
                        {
                            if (xx==0 && yy==0) continue;

                            if (IsValidPosition(Mathf.RoundToInt(position.x) + xx, Mathf.RoundToInt(position.y) + yy))
                            {
                                var neighborTile = GetTileAtPos(position.x + xx, position.y + yy);

                                if (connectedTiles.Contains(neighborTile)) continue;

                                if (neighborTile.OccupiedBallUnit != null)
                                {
                                    if (neighborTile.OccupiedBallUnit.specialType == SpecialUnit.Bomb) {minesweeper=true;}
                                    neighbors.Add(neighborTile);
                                }
                            }
                        }
                    }
                    #endregion
                                    
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
        if (tile.OccupiedBallUnit.specialType == SpecialUnit.Crate) return false;

        if (pivotTile.OccupiedBallUnit.Type != tile.OccupiedBallUnit.Type) return false;

        return true;
    }

    public bool IsValidPosition(int x, int y)
    {
        if (x < 0 || y < 0) return false;
        if (x >= width || y >= height) return false;
        return true;
    }

    public void RestartAllTiles()
    {
        foreach(KeyValuePair<Vector2Int, Tile> cell in cells)
        {
            cell.Value.RefreshTile();
        }
    }



    // a fun hidden input
    private KeyCode[] nukeSequence = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
    };
    private int sequenceIndex;
    
    private void Update() {
        if (Input.GetKeyDown(nukeSequence[sequenceIndex])) {
            if (++sequenceIndex == nukeSequence.Length){
                sequenceIndex = 0;
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
    }
}