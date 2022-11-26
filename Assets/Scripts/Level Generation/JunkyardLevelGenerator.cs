using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct LevelObjectData
{
    public GameObject levelObject;
    public float weight;
}

[RequireComponent(typeof(LevelMeshController))]
public class JunkyardLevelGenerator : LevelGeneratorBase
{
    public Vector2Int resolution;

    public int itemCount;
    public List<LevelObjectData> levelObjects;

    private LevelMeshController _levelMeshController;
    private Grid _currentGrid;
    private Perlin _terrainPerlin;

    private float _weightSum;

    private Transform _itemsParent;

    private void Awake()
    {
        _weightSum = -1f;
        _terrainPerlin = new Perlin(31);

        _itemsParent = (new GameObject("Items")).transform;
        _itemsParent.parent = transform.parent;
    }

    void Start()
    {
        _levelMeshController = GetComponent<LevelMeshController>();

        GenerateGrid();
        GenerateItems();
    }

    public void GenerateGrid()
    {
        int width = resolution.x;
        int height = resolution.y;
        Grid grid = new Grid(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float noiseValue = GetNoiseValueAt(x, z);

                if (noiseValue <= 0.5f)
                {
                    noiseValue = 0f;
                }
                else
                {
                    noiseValue = RangeUtilities.map(noiseValue, 0.5f, 1f, 0f, 1f);
                }

                int terrainHeight = Mathf.FloorToInt(noiseValue * height);
                for (int y = 0; y < terrainHeight; y++)
                {
                    grid.GetCell(x, y, z).SetValue(noiseValue).SetFilled(true);
                }
            }
        }

        _currentGrid = grid;
        _levelMeshController.Generate(_currentGrid);
        _levelMeshController.UpdateMeshes();
    }

    public void GenerateItems()
    {
        if (_weightSum < 0)
        {
            CalculateWeights();
        }

        for (int i = 0; i < itemCount; i++)
        {
            GameObject selectedItem = ChooseRandomItem();
            Debug.Assert(selectedItem != null);
            GameObject newItem = Instantiate(selectedItem, GenerateRandomPosition(), GenerateRandomRotation(), _itemsParent);

        }


    }

    private Vector3 GenerateRandomPosition()
    {
        float x = Random.Range(-HalfLevelSizeInMeters.x, HalfLevelSizeInMeters.x);
        float z = Random.Range(-HalfLevelSizeInMeters.z, HalfLevelSizeInMeters.z);
        float y = Random.Range(GetNoiseValueFromWorldPos(x, z), HalfLevelSizeInMeters.y);
        return new Vector3(x, y, z);
    }

    private Quaternion GenerateRandomRotation()
    {
        return Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)); ;
    }


    private void CalculateWeights()
    {
        float weightSum = 0;
        foreach (LevelObjectData data in levelObjects)
        {
            weightSum += data.weight;
        }
        _weightSum = weightSum;
    }

    private GameObject ChooseRandomItem()
    {
        Debug.Assert(_weightSum > 0f);

        float randomValue = Random.Range(0f, _weightSum);
        int currIndex = -1;
        while (randomValue > 0)
        {
            currIndex++;
            randomValue -= levelObjects[currIndex].weight;
        }

        return levelObjects[currIndex].levelObject;
    }

    public float GetJunkHeight(int x, int z)
    {
        return GetNoiseValueAt(x, z) * levelSizeInMeters.y;
    }

    public float GetNoiseValueFromWorldPos(float x, float z)
    {
        Vector3Int gridPos = _levelMeshController.GetGridAddressOfWorldPos(new Vector3(x, 0f, z) + HalfLevelSizeInMeters);
        return GetNoiseValueAt(gridPos.x, gridPos.z);
    }

    public float GetNoiseValueAt(Vector2Int point)
    {
        return GetNoiseValueAt(point.x, point.y);
    }

    public float GetNoiseValueAt(int x, int z)
    {
        return _terrainPerlin.Noise2D(x, z, 0.155f, 0.9f, 3);
    }
}
