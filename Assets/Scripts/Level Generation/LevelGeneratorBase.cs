using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelGenerationSpeed
{
    None,
    Slow,
    Fast
}

public abstract class LevelGeneratorBase : MonoBehaviour
{
    protected LevelMeshController levelMeshController;

    public event GenerationCompletion OnGenerationCompleted;

    public LevelGenerationSpeed GenerationSpeed { 
        get
        {
            return _generationSpeed;
        }
        set
        {
            Debug.Assert(value != LevelGenerationSpeed.None);
            _generationSpeed = value;
        }
    }

    protected Perlin terrainPerlin;

    public Vector2Int levelSizeInMeters;

    public Vector3 HalfLevelDimensionsInMeters
    {
        get
        {
            return (new Vector3(levelSizeInMeters.x, levelSizeInMeters.y, levelSizeInMeters.x)) / 2f;
        }
    }

    private LevelGenerationSpeed _generationSpeed;

    protected void Awake()
    {
        levelMeshController = GetComponent<LevelMeshController>();

        terrainPerlin = new Perlin(31);
        ResetGenerationSpeed();
    }


    public float GetNoiseValueAt(Vector2 point)
    {
        return GetNoiseValueAt(point.x, point.y);
    }

    /// <summary>
    /// Use with global positions.
    /// </summary>
    public float GetNoiseValueAt(float x, float z)
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

    public float GetLevelHeightAt(Vector3 worldPos)
    {
        return GetLevelHeightAt(worldPos.x, worldPos.z);
    }

    public abstract float GetLevelHeightAt(float x, float z);

    public abstract Vector3 GenerateRandomPositionOnLevel();
    public Quaternion GenerateRandomRotation()
    {
        return Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)); ;
    }

    protected void CallOnGenerationCompleted()
    {
        if (OnGenerationCompleted != null)
        {
            OnGenerationCompleted();

        }

        ResetGenerationSpeed();
    }

    private void ResetGenerationSpeed()
    {
        GenerationSpeed = LevelGenerationSpeed.Slow;
    }

    public delegate void GenerationCompletion();
}
