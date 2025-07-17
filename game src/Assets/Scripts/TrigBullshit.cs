using System;
using UnityEngine;

public static class TrigBullshit
{
    public static int GetQuadrant(Vector3 vec)
    {
        if (vec.y >= 0)
            return vec.x >= 0 ? 1 : 2;

        return vec.x >= 0 ? 4 : 3;
    }

    public static int GetQuadrantFromAngle(float angle)
    {
        if (0 <= angle && angle < Mathf.PI / 2) return 1;
        if (Mathf.PI / 2 <= angle && angle <= Mathf.PI) return 2;
        if (-Mathf.PI <= angle && angle < -Mathf.PI / 2) return 3;
        return 4;
    }

    public static float GetAngle(Vector3 vec)
    {
        float smallAngle = Mathf.Atan(vec.y / vec.x);

        switch (GetQuadrant(vec))
        {
            case 1:
            case 4:
                return smallAngle;
            case 2:
                return Mathf.PI + smallAngle;
            case 3:
                return smallAngle - Mathf.PI;
            default:
                throw new Exception("what the fuck");
        }
    }
    public static float GetAngleBetween(Vector3 initial, Vector3 final)
    {
        return GetAngle(final - initial);
    }
}
