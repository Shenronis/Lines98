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
    private Dictionary<Vector2Int, Tile> cells;
    private Pathfinding pathfinder;

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

        mainCamera.position = new Vector3((float)width/2 -0.5f, (float)height/2 -0.5f, -10f);      
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

    public Tile GetRandomTilePosition()
    {        
        var tile = cells.Where(t => t.Key.x < width && t.Key.y < height  && t.Value.Spawnable).OrderBy(t => Random.value).First().Value;

        // No more tile to spawn from
        if (tile == null)
        {
            GameManager.Instance.ChangeState(GameState.Lose);
        }

        return tile;
    }

    public void UpdatePathdinding()
    {
        pathfinder.ReEvaluatePathdinding();
    }

    public Vector3[] findPath(Vector2 start, Vector2 end)
    {
        return pathfinder.findPath(start, end);
    }

    public List<Tile> checkLines(Vector2 pivot)
    {                
        List<Tile> connectedTiles = new List<Tile>();
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
                if (tile.OccupiedBallUnit == null) break;                
                if (pivotTile.OccupiedBallUnit.Type != tile.OccupiedBallUnit.Type) break;

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
                if (tile.OccupiedBallUnit == null) break;                
                if (pivotTile.OccupiedBallUnit.Type != tile.OccupiedBallUnit.Type) break;

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

        return connectedTiles;
    }

    private bool IsValidPosition(int x, int y)
    {
        if (x < 0 || y < 0) return false;
        if (x >= width || y >= height) return false;
        return true;
    }
}