using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ThrowingLevelGenerator : LevelGeneratorBase
{
    public int itemCountPerRate;
    public List<LevelObjectData> levelObjects;
    private Perlin _terrainPerlin;

    private float _weightSum;

    private Transform _itemsParent;

    private new void Awake()
    {
        _weightSum = -1f;
        _terrainPerlin = new Perlin(31);

        _itemsParent = (new GameObject("Items")).transform;
        _itemsParent.parent = transform.parent;
    }

    void Start()
    {
        GenerateItems();
    }

    public void GenerateItems()
    {
        if (_weightSum < 0)
        {
            CalculateWeights();
        }

        for (float x = 0; x < levelSizeInMeters.x; x += 5)
        {
            for (float z = 0; z < levelSizeInMeters.z; z += 5)
            {
                float noiseValue = GetPerlinAtWorldPos(x, z);
                float spawnItemCount = noiseValue * itemCountPerRate;
                for (int i = 0; i < spawnItemCount; i++)
                {
                    GameObject selectedItem = ChooseRandomItem();
                    GameObject newItem = InstantiateItem(selectedItem, x, z);
                }
            }
        }


    }

    private GameObject InstantiateItem(GameObject selectedItem, float x, float z)
    {
        Vector3 selectedPos = new Vector3(x, GetPerlinAtWorldPos(x, z) * levelSizeInMeters.y + selectedItem.transform.localScale.y, z);
        Vector3 halfLevelSizeInMeters = HalfLevelSizeInMeters;
        selectedPos.x -= halfLevelSizeInMeters.x;
        selectedPos.z -= halfLevelSizeInMeters.z;

        while (Physics.CheckBox(selectedPos, selectedItem.transform.lossyScale / 2))
        {
            selectedPos.y += 5f;
        }
        return Instantiate(selectedItem, selectedPos, GenerateRandomRotation(), _itemsParent);
    }

    public Vector3 GenerateRandomPosition()
    {
        Vector3 halfLevelSizeInMeters = HalfLevelSizeInMeters;
        float x = Random.Range(-halfLevelSizeInMeters.x, halfLevelSizeInMeters.x);
        float z = Random.Range(-halfLevelSizeInMeters.z, halfLevelSizeInMeters.z);
        float y = Random.Range(GetLevelHeightAtWorldPos(new Vector3(x, 0f, z)) + 5f, levelSizeInMeters.y);
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

    public float GetPerlinAtWorldPos(Vector2 point)
    {
        return GetPerlinAtWorldPos(point.x, point.y);
    }

    public float GetPerlinAtWorldPos(float x, float z)
    {
        return _terrainPerlin.Noise2D(x, z, 0.155f, 0.9f, 3);
    }

    public override float GetLevelHeightAtWorldPos(float x, float z)
    {
        return GetPerlinAtWorldPos(x, z);
    }
}
