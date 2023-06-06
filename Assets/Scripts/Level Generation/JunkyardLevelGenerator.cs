using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WIAFN.LevelGeneration;
using Random = UnityEngine.Random;

[System.Serializable]
public struct LevelObjectData
{
    [HideInInspector]
    public string name;

    public GameObject levelObject;
    public float weight;
}

[RequireComponent(typeof(LevelMeshController))]
public class JunkyardLevelGenerator : LevelGeneratorBase
{
    private class GridGenerationOperation
    {
        public Task task;
        public CancellationTokenSource cancellationTokenSource;
        public GridGenerator generator;

        public GridGenerationOperation(Task task, GridGenerator generator, CancellationTokenSource ct)
        {
            this.task = task;
            this.generator = generator;
            this.cancellationTokenSource = ct;
        }
    }

    public Vector2Int resolution;

    public int itemCount;
    public List<LevelObjectData> levelObjects;

    private Grid _currentGrid;

    private float _weightSum;

    private Transform _itemsParent;
    private GridGenerationOperation _waitingForOperation;

    private ItemPoolGenerator _itemPoolGenerator;

    private new void Awake()
    {
        base.Awake();

        _weightSum = -1f;

        _itemsParent = (new GameObject("Items")).transform;
        _itemsParent.parent = transform.parent;
        _itemsParent.transform.localPosition = Vector3.zero;

        levelMeshController.OnLevelMeshGenerated += OnLevelMeshGenerated;
    }

    void Start()
    {
        _waitingForOperation = null;
        _itemPoolGenerator = new ItemPoolGenerator(itemCount);

        GenerateLevel();
        GenerateItemPool();
    }

    private void Update()
    {
        if (_waitingForOperation != null && _waitingForOperation.task.IsCompleted)
        {
            GridGenerationOperation gridGenerationOp = _waitingForOperation;
            _waitingForOperation = null;

            _currentGrid = gridGenerationOp.generator.grid;

            levelMeshController.Initialize(_currentGrid.Size);
            levelMeshController.Generate(_currentGrid, true);
        }

    }

    private void OnDestroy()
    {
        levelMeshController.OnLevelMeshGenerated -= OnLevelMeshGenerated;
        _waitingForOperation?.cancellationTokenSource.Cancel();

        _itemPoolGenerator?.DestroyItems();
        _itemPoolGenerator = null;
    }

    public void GenerateLevel()
    {
        //GenerateGrid();
        GenerateGridMultithreaded();
    }

    private void OnLevelMeshGenerated()
    {
        StartCoroutine(MoveItemsOnItemPoolIsReady());

        CallOnGenerationCompleted();
    }

    public void GenerateGrid()
    {
        //int width = resolution.x;
        //int height = resolution.y;

        Vector3 halfLevelSize = HalfLevelDimensionsInMeters;
        GridGenerator gridGenerator = new GridGenerator(this, levelSizeInMeters.x, levelSizeInMeters.y, resolution, new Vector3(-halfLevelSize.x, 0f, -halfLevelSize.z));
        levelMeshController.Initialize(new Vector3Int(resolution.x, resolution.y, resolution.x));

        gridGenerator.GenerateGrid();
        Grid grid = gridGenerator.grid;

        _currentGrid = grid;
        levelMeshController.Generate(_currentGrid, multithreaded: false);
    }

    public void GenerateGridMultithreaded()
    {
        Vector3 halfLevelSize = HalfLevelDimensionsInMeters;
        GridGenerator gridGenerator = new GridGenerator(this, levelSizeInMeters.x, levelSizeInMeters.y, resolution, new Vector3(-halfLevelSize.x, 0f, -halfLevelSize.z));
        levelMeshController.Initialize(new Vector3Int(resolution.x, resolution.y, resolution.x));

        var ctSource = new CancellationTokenSource();
        Task chunkTask = Task.Factory.StartNew(gridGenerator.GenerateGrid, ctSource.Token);
        _waitingForOperation = new GridGenerationOperation(chunkTask, gridGenerator, ctSource);
    }

    #region Item Generation
    private void GenerateItemPool()
    {
        if (_weightSum < 0)
        {
            CalculateWeights();
        }

        _itemPoolGenerator.StartGeneration(this, ChooseRandomItem, true, _itemsParent);
    }

    private IEnumerator MoveItemsOnItemPoolIsReady()
    {
        while (!_itemPoolGenerator.IsCompleted)
        {
            yield return null;
        }

        foreach (GameObject item in _itemPoolGenerator.Pool)
        {
            item.transform.position = GenerateRandomPositionOnLevel(yRandom: true);
            item.transform.rotation = GenerateRandomRotation();
            item.SetActive(true);
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
    #endregion // Item Generation

    public override float GetLevelHeightAt(float x, float z)
    {
        return transform.parent.position.y + Mathf.Max(0f, GetNoiseValueAt(x, z) * levelSizeInMeters.y);
    }

    //public Grid CurrentGrid => _currentGrid;

    #region GUI
    // To name elements according to prefab names.
    private void OnValidate()
    {
        for (int i = 0; i < levelObjects.Count; i++)
        {
            var levelObjectData = levelObjects[i];
            string name = levelObjectData.levelObject == null || string.IsNullOrWhiteSpace(levelObjectData.levelObject.name) ? "": levelObjectData.levelObject.name;

            levelObjectData.name = name;
            levelObjects[i] = levelObjectData;
        }
    }
    #endregion //GUI
}
