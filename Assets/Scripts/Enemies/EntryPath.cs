using System.Collections.Generic;
using UnityEngine;

public enum EntryPattern
{
    LeftArc,
    RightArc,
    TopLoop
}

public static class EntryPath
{
    private const int SamplesPerSegment = 14;
    private const float OffscreenMargin = 1.5f;

    public static int PatternCount => 3;

    public static EntryPattern PatternAt(int index)
    {
        return (EntryPattern)(((index % PatternCount) + PatternCount) % PatternCount);
    }

    public static List<Vector3> Build(EntryPattern pattern, Vector2 target, Camera cam)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector2 center = cam.transform.position;

        List<Vector2> waypoints = new List<Vector2>();

        switch (pattern)
        {
            case EntryPattern.LeftArc:
                waypoints.Add(new Vector2(center.x - halfWidth - OffscreenMargin, center.y + halfHeight * 0.4f));
                waypoints.Add(new Vector2(center.x - halfWidth * 0.6f, center.y - halfHeight * 0.2f));
                waypoints.Add(new Vector2(center.x - halfWidth * 0.1f, center.y + halfHeight * 0.1f));
                break;
            case EntryPattern.RightArc:
                waypoints.Add(new Vector2(center.x + halfWidth + OffscreenMargin, center.y + halfHeight * 0.4f));
                waypoints.Add(new Vector2(center.x + halfWidth * 0.6f, center.y - halfHeight * 0.2f));
                waypoints.Add(new Vector2(center.x + halfWidth * 0.1f, center.y + halfHeight * 0.1f));
                break;
            default:
                waypoints.Add(new Vector2(center.x, center.y + halfHeight + OffscreenMargin));
                waypoints.Add(new Vector2(center.x + halfWidth * 0.45f, center.y + halfHeight * 0.55f));
                waypoints.Add(new Vector2(center.x + halfWidth * 0.5f, center.y + halfHeight * 0.05f));
                waypoints.Add(new Vector2(center.x + halfWidth * 0.1f, center.y - halfHeight * 0.1f));
                break;
        }

        waypoints.Add(target);
        return Sample(waypoints);
    }

    public static List<Vector3> BuildDive(Vector3 start, Vector3 playerPosition, Camera cam)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector2 center = cam.transform.position;
        float swing = start.x <= playerPosition.x ? -1f : 1f;

        float swingX = Mathf.Clamp(start.x + swing * 2.5f, center.x - halfWidth + 0.8f, center.x + halfWidth - 0.8f);
        float swingY = Mathf.Lerp(start.y, playerPosition.y, 0.35f);

        List<Vector2> waypoints = new List<Vector2>
        {
            start,
            new Vector2(swingX, swingY),
            playerPosition,
            new Vector2(playerPosition.x - swing * 1.2f, center.y - halfHeight - OffscreenMargin)
        };

        return Sample(waypoints);
    }

    public static float Length(IList<Vector3> points)
    {
        float length = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            length += Vector3.Distance(points[i - 1], points[i]);
        }

        return length;
    }

    public static List<Vector3> Sample(IList<Vector2> controlPoints)
    {
        List<Vector3> result = new List<Vector3>();
        int last = controlPoints.Count - 1;

        for (int i = 0; i < last; i++)
        {
            Vector2 p0 = controlPoints[Mathf.Max(i - 1, 0)];
            Vector2 p1 = controlPoints[i];
            Vector2 p2 = controlPoints[i + 1];
            Vector2 p3 = controlPoints[Mathf.Min(i + 2, last)];

            for (int s = 0; s < SamplesPerSegment; s++)
            {
                float t = (float)s / SamplesPerSegment;
                result.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        result.Add(controlPoints[last]);
        return result;
    }

    private static Vector3 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector2 point = 0.5f * (2f * p1
            + (p2 - p0) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (3f * p1 - p0 - 3f * p2 + p3) * t3);

        return new Vector3(point.x, point.y, 0f);
    }
}
