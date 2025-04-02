using System.Collections.Generic;
using UnityEngine;

public class GroundPointsHandler : MonoBehaviour
{
    private static GroundPointsHandler self;
    private List<AIPoint> addedPoints = new List<AIPoint>();

    [SerializeField] private List<AIPoint> points = new List<AIPoint>();

    private void Awake()
    {
        self = this;
    }

    public static void ClearStartPos()
    {
        if (self)
        {
            self.addedPoints.Clear();
        }
    }

    public static AIPoint GetStartPoint()
    {
        var random = Random.Range(0, self.points.Count);
        if(self.addedPoints.Contains(self.points[random]))
        {
            return GetStartPoint();
        }
        self.addedPoints.Add(self.points[random]);
        return self.points[random];
    }
}
