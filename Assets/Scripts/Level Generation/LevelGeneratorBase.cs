using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelGenerationSpeed
{
    None,
    Slow,
    Fast,
    SuperFast
}

public abstract class LevelGeneratorBase : MonoBehaviour
{
    protected LevelMeshController levelMeshController;

    public bool GenerationComplete { get; protected set; }

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

        terrainPerlin = new Perlin();
        ResetGenerationSpeed();

        GenerationComplete = false;
    }

    public float GetNoiseValueAt(Vector2 point, bool realWorldPos = true)
    {
        return GetNoiseValueAt(point.x, point.y, realWorldPos: realWorldPos);
    }

    /// <summary>
    /// Use with global positions.
    /// </summary>
    public float GetNoiseValueAt(float x, float z, bool realWorldPos = true)
    {
        if (realWorldPos)
        {
            x = x - transform.parent.position.x;
            z = z - transform.parent.position.z;
        }

        float firstNoise = terrainPerlin.Noise2D(x, z, 0.09f, 0.9f, 2);
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


    public Vector3 GenerateRandomPositionOnLevel(bool towardsMiddle = false, bool yRandom = false)
    {
        Vector3 halfLevelDimensions = HalfLevelDimensionsInMeters;
        if (towardsMiddle)
        {
            halfLevelDimensions /= 2;
        }

        float x = Random.Range(transform.parent.position.x - halfLevelDimensions.x, transform.parent.position.x + halfLevelDimensions.x);
        float z = Random.Range(transform.parent.position.z - halfLevelDimensions.z, transform.parent.position.z + halfLevelDimensions.z);
        float y;
        if (yRandom)
        {
            y = transform.parent.position.y + Random.Range(GetLevelHeightAt(x, z) + 5f, levelSizeInMeters.y);
        }
        else
        {
            y = transform.parent.position.y + GetLevelHeightAt(x, z);
        }

        return new Vector3(x, y, z);
    }

    public Vector3 GenerateRandomPositionOnGround(bool towardsMiddle = false)
    {
        int stepCount = 0;
        while (stepCount < 2000)
        {
            Vector3 randomPos = GenerateRandomPositionOnLevel(towardsMiddle: towardsMiddle);
            if (GetNoiseValueAt(randomPos.x, randomPos.z) < 0.1f)
            {
                return randomPos;
            }

            stepCount++;
        }

        return transform.parent.position + Vector3.up * 5f;
    }

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
