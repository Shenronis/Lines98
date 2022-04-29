using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    #region Singleton
    public static Pathfinding Instance {get; private set;}
    void Awake()
    {
        Instance = this;
    }
    #endregion
    
    List<Tuple<PathNode, Tile>> grid;

    public Pathfinding(Dictionary<Vector2Int, Tile> cells, int width, int height)
    {
        grid = new List<Tuple<PathNode, Tile>>();

        // Initialize grid for pathfinding
        foreach(KeyValuePair<Vector2Int, Tile> cell in cells)
        {
            var position = cell.Key;
            var tile = cell.Value;
            var node = new PathNode(position.x, position.y);
            node.isWalkable = tile.Walkable;

            grid.Add(new Tuple<PathNode, Tile>(node, tile));
        }

        // Add neighbors
        foreach(var tuple in grid)
        {            
            var position =  tuple.Item1;
            List<PathNode> neighbors = new List<PathNode>();
    
            // (Up) (Up) (Down) (Down) (Left) (Right) (Left) (Right) [B] [A] [start]

            // Find neighbor
            // Up
            if (position.y + 1 < height) neighbors.Add(GetPathNodeXY(position.x, position.y + 1));
            // Down
            if (position.y - 1 >= 0) neighbors.Add(GetPathNodeXY(position.x, position.y - 1));            
            // Left
            if (position.x - 1 >= 0) neighbors.Add(GetPathNodeXY(position.x - 1, position.y));
            // Right
            if (position.x + 1 < width) neighbors.Add(GetPathNodeXY(position.x + 1, position.y));            

            tuple.Item1.neighbors = neighbors;
        }
    }

    /// <summary>
    /// Return a PathNode at a given coordinate x,y
    /// </summary>
    /// <param name="x">Position's x</param>
    /// <param name="y">Position's x</param>
    /// <returns>PathNode</returns>
    private PathNode GetPathNodeXY(int x, int y)
    {
        var searchPos = new Vector2(x,y);
        return grid.Find(t => t.Item1.position == searchPos).Item1;
    }

    /// <summary>
    /// Update grid to set each PathNode.isWalkable
    /// </summary>
    public void UpdatePathdinding()
    {
        foreach(var tuple in grid)
        {            
            var tile = tuple.Item2;
            
            // Set PathNode.isWalkable => tile.Walkable == true
            tuple.Item1.isWalkable = tile.Walkable;
        }
    }

    /// <summary>
    /// Return if the given start, end PathNode is valid
    /// </summary>
    /// <param name="start">Starting PathNode</param>
    /// <param name="end">End PathNode</param>
    /// <returns>boolean</returns>
    private bool IsValidPath(PathNode start, PathNode end)
    {
        if ((start==null) || (end==null)) {return false;}
        return true;
    }

    /// <summary>
    /// Backtrack from the given List<> result from the pathfinding algorithm
    /// </summary>
    /// <param name="end"></param>
    /// <returns></returns>
    private List<PathNode> BacktrackToPath(PathNode end)
    {        
        List<PathNode> path = new List<PathNode>();
        path.Add(end);
        PathNode current = end;

        while (current.prev != null)
        {
            path.Add(current.prev);
            current = current.prev;
        }

        path.Reverse();

        return path;
    }

    /// <summary>
    /// BFS Pathfinding algorithm
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">End position</param>
    /// <returns>List<PathNode></returns>
    public List<PathNode> findPath_BFS(Vector2 start, Vector2 end)
    {
        HashSet<PathNode> visited = new HashSet<PathNode>();
        Queue<PathNode> frontier = new Queue<PathNode>();        

        PathNode Start=null, End=null;
        bool shouldBypass = false;

        foreach(var tuple in grid)
        {            
            var position = tuple.Item1;
            tuple.Item1.prev = null;

            if (position.x == start.x && position.y == start.y) { 
                Start = tuple.Item1;
                shouldBypass = (tuple.Item2.OccupiedBallUnit.specialType == BallUnitSpecial.Ghost)
                            || (tuple.Item2.OccupiedBallUnit.specialType == BallUnitSpecial.Pacman);
            }
            else if (position.x == end.x && position.y == end.y) {
                End = tuple.Item1; 
            }
        }      

        if (!IsValidPath(Start, End)) {return null;}

        visited.Add(Start);
        frontier.Enqueue(Start);

        while(frontier.Count > 0)
        {
            PathNode current = frontier.Dequeue();

            if (current == End) {break;}

            foreach(var neighbor in current.neighbors)
            {                
                if (!visited.Contains(neighbor) && (neighbor.isWalkable || shouldBypass))
                {
                    visited.Add(neighbor);
                    frontier.Enqueue(neighbor);

                    neighbor.prev = current;                    
                }
            }            
        }

        List<PathNode> path = BacktrackToPath(End);
        return path;
    }

    /// <summary>
    /// A wrapper to return Vector3[] from pathfinding algorithm
    /// The reason is DOTween.DOPath() needs to be feeded with Vector3[]
    /// </summary>
    /// 
    /// <example>
    /// DOPath(Vector3[] waypoints, float duration, PathType pathType = Linear, PathMode pathMode = Full3D, int resolution = 10, Color gizmoColor = null)
    /// </example>
    /// 
    /// <param name="start">Starting position</param>
    /// <param name="end">End position</param>
    /// <returns>Vector3[]</returns>
    public Vector3[] findPath(Vector2 start, Vector2 end)
    {
        var path = findPath_BFS(start, end);
        if (path != null)
        {
            Vector3[] vectorPath = new Vector3[path.Count];
            
            for (int i = 0; i < path.Count; i++)
            {
                var node = path[i];
                vectorPath[i] = new Vector3(node.x, node.y);
            }

            return vectorPath;
        }
        else
        {
            return null;
        }
    }
}