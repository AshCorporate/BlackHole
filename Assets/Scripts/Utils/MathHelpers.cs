using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static utility class with geometry and math helpers used across the project.
/// </summary>
public static class MathHelpers
{
    /// <summary>
    /// Returns a random point inside a circle of the given radius centred at the origin.
    /// </summary>
    public static Vector2 RandomPointInCircle(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float r = radius * Mathf.Sqrt(Random.Range(0f, 1f));
        return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
    }

    /// <summary>
    /// Returns a random point on the circumference of a circle.
    /// </summary>
    public static Vector2 RandomPointOnCircle(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
    }

    /// <summary>
    /// Clamps a Vector2 so that its magnitude does not exceed maxLength.
    /// </summary>
    public static Vector2 ClampMagnitude(Vector2 v, float maxLength)
    {
        if (v.sqrMagnitude > maxLength * maxLength)
            return v.normalized * maxLength;
        return v;
    }

    /// <summary>
    /// Checks whether a point is inside a circle defined by centre and radius.
    /// </summary>
    public static bool IsInsideCircle(Vector2 point, Vector2 centre, float radius)
    {
        return (point - centre).sqrMagnitude <= radius * radius;
    }

    /// <summary>
    /// Computes the signed area of a polygon given its vertices (counter-clockwise = positive).
    /// Used for territory capture area calculation.
    /// </summary>
    public static float PolygonSignedArea(List<Vector2> polygon)
    {
        int n = polygon.Count;
        float area = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % n];
            area += a.x * b.y - b.x * a.y;
        }
        return area / 2f;
    }

    /// <summary>
    /// Checks whether a point is inside a polygon using the ray-casting algorithm.
    /// </summary>
    public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int n = polygon.Count;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];
            if (((vi.y > point.y) != (vj.y > point.y)) &&
                (point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    /// <summary>
    /// Linearly interpolates a float, used for smooth UI transitions.
    /// </summary>
    public static float SmoothLerp(float current, float target, float speed, float deltaTime)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * deltaTime));
    }

    /// <summary>
    /// Formats seconds into mm:ss string.
    /// </summary>
    public static string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", mins, secs);
    }
}
