using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WIAFN.LevelGeneration
{
    [RequireComponent(typeof(LevelMeshController))]
    public class SingleThreadedJunkyardLevelGeneratorBackup : LevelGeneratorBase
    {
        public Vector2Int resolution;

        public int itemCount;
        public List<LevelObjectData> levelObjects;

        private Grid _currentGrid;
        private Perlin _terrainPerlin;

        private float _weightSum;

        private Transform _itemsParent;

        private new void Awake()
        {
            base.Awake();

            _weightSum = -1f;
            _terrainPerlin = new Perlin(31);

            _itemsParent = (new GameObject("Items")).transform;
            _itemsParent.parent = transform.parent;
            _itemsParent.transform.localPosition = Vector3.zero;
        }

        void Start()
        {
            GenerateLevel();
        }

        public void GenerateLevel()
        {
            GenerateGrid();
            GenerateItems();

            CompleteGeneration();
        }

        private void CompleteGeneration()
        {
            GenerationComplete = true;
            CallOnGenerationCompleted();
        }

        public void GenerateGrid()
        {
            int width = resolution.x;
            int height = resolution.y;
            Grid grid = new Grid(width, height);
            levelMeshController.Initialize(grid.Size);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    float noiseValue = GetNoiseValueAt(x, z);

                    int terrainHeight = Mathf.FloorToInt(noiseValue * height);
                    for (int y = 0; y < terrainHeight; y++)
                    {
                        //grid.GetCell(x, y, z).SetValue(noiseValue).SetFilled(true);
                        float heightValue = (height - y) / height;
                        grid.GetCell(x, y, z).SetValue(RangeUtilities.map(heightValue, 0f, 1f, 0.5f, 1f));
                    }

                    float heightDiff = height - terrainHeight;
                    for (int y = 0; y < heightDiff; y++)
                    {
                        float heightValue = (heightDiff - y) / heightDiff;
                        grid.GetCell(x, terrainHeight + y, z).SetValue(RangeUtilities.map(heightValue, 0f, 1f, 0f, 0.5f));
                    }
                }
            }

            _currentGrid = grid;
            levelMeshController.Generate(_currentGrid);
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
                GameObject newItem = Instantiate(selectedItem, GenerateRandomPositionOnLevel(yRandom: true), GenerateRandomRotation(), _itemsParent);

            }


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

        public float GetPileHeightAtWorldPos(float x, float z)
        {
            return GetNoiseValueFromWorldPos(x, z) * levelSizeInMeters.y;
        }

        public float GetPileHeight(int x, int z)
        {
            return GetNoiseValueAt(x, z) * levelSizeInMeters.y;
        }

        public float GetNoiseValueFromWorldPos(float x, float z)
        {
            Vector3Int gridPos = levelMeshController.GetGridAddressOfWorldPos(new Vector3(x, 0f, z));
            return GetNoiseValueAt(gridPos.x, gridPos.z);
        }

        public override float GetLevelHeightAt(float x, float z)
        {
            return transform.parent.position.y + GetNoiseValueFromWorldPos(x, z);
        }

        public Grid CurrentGrid => _currentGrid;

        #region GUI
        // To name elements according to prefab names.
        private void OnValidate()
        {
            for (int i = 0; i < levelObjects.Count; i++)
            {
                var levelObjectData = levelObjects[i];
                string name = levelObjectData.levelObject == null || string.IsNullOrWhiteSpace(levelObjectData.levelObject.name) ? "" : levelObjectData.levelObject.name;

                levelObjectData.name = name;
                levelObjects[i] = levelObjectData;
            }
        }
        #endregion //GUI

    }
}
