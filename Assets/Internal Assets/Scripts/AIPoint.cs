using System.Collections.Generic;
using UnityEngine;

public class AIPoint : MonoBehaviour
{
    public List<AIPoint> points = new List<AIPoint>();
    

    public AIPoint GetPoint(AIPoint lastPoint)
    {
        int random = Random.Range(0, points.Count);
        if (points[random] == lastPoint )
        {
            return GetPoint(lastPoint);
        }
        return points[random];
    }
}
