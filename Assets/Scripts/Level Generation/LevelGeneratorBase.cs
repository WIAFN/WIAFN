using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorBase : MonoBehaviour
{
    protected Perlin terrainPerlin;

    public Vector3 levelSizeInMeters;

    public Vector3 HalfLevelSizeInMeters
    {
        get
        {
            return levelSizeInMeters / 2;
        }
    }

    protected void Awake()
    {
        terrainPerlin = new Perlin(31);
    }


    public float GetNoiseValueAt(Vector2Int point)
    {
        return GetNoiseValueAt(point.x, point.y);
    }

    public float GetNoiseValueAt(int x, int z)
    {
        float firstNoise = terrainPerlin.Noise2D(x, z, 0.11f, 0.9f, 2);
        firstNoise -= 0.5f;
        firstNoise = Mathf.Clamp01(firstNoise);
        firstNoise *= 2f;

        //firstNoise = Mathf.Sin(RangeUtilities.map(firstNoise, 0f, 1f, 0f, Mathf.PI / 2f));
        firstNoise = Mathf.Sin(firstNoise * Mathf.PI / 2f);

        //float result = Mathf.Clamp(firstNoise, 0f, 1f);
        float secondNoise = terrainPerlin.Noise2D(x, z, 0.1f, 0.8f, 1);
        secondNoise = Mathf.Max(secondNoise - 0.3f, 0f); ;
        float result = Mathf.Clamp01(firstNoise + 0.1f * secondNoise);

        return result;
    }
}
