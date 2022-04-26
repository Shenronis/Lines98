using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int x,y;
    public Vector2 position;
    public bool isWalkable;
    public PathNode prev;
    public List<PathNode> neighbors;

    public PathNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.position = new Vector2(x,y);
        this.isWalkable = true;
        this.neighbors = new List<PathNode>();
    }
}
