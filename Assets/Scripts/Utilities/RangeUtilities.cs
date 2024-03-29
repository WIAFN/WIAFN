using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RangeUtilities
{
    public static float map(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    public static float map01(float x, float inMin, float inMax)
    {
        return map(x, inMin, inMax, 0f, 1f);
    }
}
