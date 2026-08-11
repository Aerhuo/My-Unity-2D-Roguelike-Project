using System;
using UnityEngine;

public static class Bresenham
{
    public static void CastRay(int x0, int y0, int x1, int y1, Func<int, int, bool> onStep)
    {
        int dx = x1 - x0, dy = y1 - y0;
        int sx = dx > 0 ? 1 : -1, sy = dy > 0 ? 1 : -1;
        dx = Mathf.Abs(dx);
        dy = Mathf.Abs(dy);
        int err = dx - dy;
        
        while (true)
        {
            if (onStep != null && !onStep(x0, y0)) break;
            if (x0 == x1 && y0 == y1) break;

            int e2 = err << 1;
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
            if (e2 > - dy)
            {
                err -= dy;
                x0 += sx;
            }
        }
    }
    public static void CastCircle(Vector2Int originPos, int radius, Func<int, int, bool> onStep)
    {
        int r2 = radius * radius;
        int minX = originPos.x - radius, minY = originPos.y - radius;
        int maxX = originPos.x + radius, maxY = originPos.y + radius;

        bool CircleStep(int x, int y)
        {
            int dx = originPos.x - x, dy = originPos.y - y;
            if (dx * dx + dy * dy > r2) return false;

            return onStep == null || onStep(x, y);
        }

        Func<int, int, bool> onCircleStep = CircleStep;
        
        for (int x = minX; x <= maxX; ++x)
        {
            CastRay(originPos.x, originPos.y, x, minY, onCircleStep);
            CastRay(originPos.x, originPos.y, x, maxY, onCircleStep);
        }

        for (int y = minY + 1; y < maxY; ++y)
        {
            CastRay(originPos.x, originPos.y, minX, y, onCircleStep);
            CastRay(originPos.x, originPos.y, maxX, y, onCircleStep);
        }
    }
}