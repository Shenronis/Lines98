using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderController : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3[] points;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void SetUpLine(Vector3[] points)
    {
        lr.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i]);
        }
    }

    public void RemoveLine()
    {
        lr.positionCount = 0;
    }    
}
