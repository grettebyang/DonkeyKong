using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugClass : MonoBehaviour
{
    public static void DrawCircle(Vector2 position, float radius, int segments, Color color)
    {
        if (radius <= 0f || segments <= 0) return;

        float angleStep = 360f / segments;
        angleStep *= Mathf.Deg2Rad;
        Vector2 lineStart = Vector2.zero;
        Vector2 lineEnd = Vector2.zero;

        for (int i = 0; i < segments; i++)
        {
            lineStart.x = Mathf.Cos(angleStep * i);
            lineStart.y = Mathf.Sin(angleStep * i);

            lineEnd.x = Mathf.Cos(angleStep * (i + 1));
            lineEnd.y = Mathf.Sin(angleStep * (i + 1));

            lineStart *= radius;
            lineEnd *= radius;

            lineStart += position;
            lineEnd += position;

            Debug.DrawLine(lineStart, lineEnd, color);
        }
    }
}
