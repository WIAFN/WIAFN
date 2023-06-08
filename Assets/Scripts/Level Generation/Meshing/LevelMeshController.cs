using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LevelGeneratorBase))]
public class LevelMeshController : MonoBehaviour
{
    // Y is constrained to 1 for multithreaded generation.
    [SerializeField]
    private Vector3Int _levelSizeInChunks;

    //public Material terrainMaterial;

    public GameObject chunkPrefab;

    [Header("Update Mesh Count Per Frame")]
    public int updateMeshCountPerFrame;
    public int fastMeshGenerationsPerFrame;
    public int slowFrameSkipsBetweenMeshGenerations;

    public event ChunkCreation OnChunkCreated;
    public event GenerationCompletion OnLevelMeshGenerated;

    public Vector3Int LevelSizeInChunks
    {
        get { return _levelSizeInChunks; }
        set 
        { 
            _levelSizeInChunks = value;
            _chunkCount = CalculateChunkCount();
        }
    }

    public ChunkMeshController[,,] ChunkGrid
    { 
        get
        {
            return _chunkGrid;
        }
    }

    public Vector3Int ChunkSizeInVoxels { get; private set; }
    public Vector3 VoxelSizeInMeters { get; private set; }

    //public int layerOfMeshes;

    public Vector3 MeshDimensionsInMeters
    {
        get
        {
            return new Vector3(_levelGenerator.levelSizeInMeters.x, _levelGenerator.levelSizeInMeters.y, _levelGenerator.levelSizeInMeters.x)/* * 4f / 3f*/;
        }
    }

    private int _chunkCount;
    public bool HasUpdatingChunks { get { return _updatingChunks.Count > 0; } }

    private LevelGeneratorBase _levelGenerator;
    private ChunkMeshController[,,] _chunkGrid;

    private Transform _generatedJunksParent;

    private int _currentFrameSkipsBetweenMeshGenerations;
    private int _currentMeshGenerationsPerFrame;

    private int _updatedMeshCountOnCurrentFrame;
    private HashSet<ChunkMeshController> _updatingChunks;

    private void Awake()
    {
        _levelGenerator = GetComponent<LevelGeneratorBase>();

        _generatedJunksParent = (new GameObject("Junks")).transform;
        _generatedJunksParent.parent = transform.parent;
        _generatedJunksParent.transform.localPosition = Vector3.zero;

        _updatingChunks = new HashSet<ChunkMeshController>();
    }

    private void Start()
    {
        Random.InitState(1);
        ResetUpdatedMeshOnFrame();
        _chunkCount = CalculateChunkCount();
    }

    private void Update()
    {
        switch (LevelGenerator.GenerationSpeed)
        {
            case LevelGenerationSpeed.Fast:
                _currentFrameSkipsBetweenMeshGenerations = 1;
                _currentMeshGenerationsPerFrame = fastMeshGenerationsPerFrame;
                break;
            case LevelGenerationSpeed.Slow:
                _currentFrameSkipsBetweenMeshGenerations = slowFrameSkipsBetweenMeshGenerations;
                _currentMeshGenerationsPerFrame = 1;
                break;
            default:
                Debug.LogAssertion("Invalid speed of level generation.");
                break;
        }
    }

    private void LateUpdate()
    {
        ResetUpdatedMeshOnFrame();
    }

    private int CalculateChunkCount()
    {
        return LevelSizeInChunks.x * LevelSizeInChunks.y * LevelSizeInChunks.z;
    }

    private void ResetUpdatedMeshOnFrame()
    {
        _updatedMeshCountOnCurrentFrame = 0;
    }

    public void Initialize(Vector3Int gridResolution)
    {
        Clear();

        Vector3 meshSizeInMeters = MeshDimensionsInMeters;
        _generatedJunksParent.localPosition = -GetHalfHorizontalMeshSizeInMeters();

        ChunkSizeInVoxels = new Vector3Int(gridResolution.x / LevelSizeInChunks.x, gridResolution.y / LevelSizeInChunks.y, gridResolution.z / LevelSizeInChunks.z);
        VoxelSizeInMeters = new Vector3(meshSizeInMeters.x / gridResolution.x, meshSizeInMeters.y / gridResolution.y, meshSizeInMeters.z / gridResolution.z);
    }

    public void Generate(Grid grid, bool multithreaded = true)
    {
        StartCoroutine(GenerateChunks(grid, multithreaded));
    }

    private IEnumerator GenerateChunks(Grid grid, bool multithreaded)
    {
        List<Task> tasks = new List<Task>();
        int _currentFrameSkip = 0;
        for (int x = 0; x < LevelSizeInChunks.x; x++)
        {
            for (int y = 0; y < LevelSizeInChunks.y; y++)
            {
                for (int z = 0; z < LevelSizeInChunks.z; z++)
                {
                    if (_currentFrameSkip > _currentMeshGenerationsPerFrame)
                    {
                        for (int i = 0; i < _currentFrameSkipsBetweenMeshGenerations; i++)
                        {
                            yield return null;
                        }

                        _currentFrameSkip = 0;
                    }

                    Vector3Int chunkAddress = new Vector3Int(x, y, z);
                    ChunkMeshController chunkController = InitiateChunk(chunkAddress);

                    Vector3Int minAddress = Vector3Int.Scale(ChunkSizeInVoxels, chunkAddress);
                    Vector3Int maxAddress = Vector3Int.Scale(ChunkSizeInVoxels, chunkAddress + Vector3Int.one) - Vector3Int.one;

                    chunkController.Generate(grid.GetSubGrid(minAddress, maxAddress), multithreaded);
                    if (multithreaded)
                    {
                        tasks.Add(chunkController.MeshGenerationTask);
                        chunkController.MeshGenerationTask.ContinueWith((previousTask) =>
                        {
                            UpdateMesh(chunkController);
                        });
                    }
                    else
                    {
                        UpdateMesh(chunkController);
                    }

                    _currentFrameSkip += 1;
                }
            }
        }

        Func<bool> whileCheckFunc;
        try
        {
            Task allWaitTask = Task.WhenAll(tasks);
            whileCheckFunc = () => { return !allWaitTask.IsCompleted; };

        } 
        catch (ArgumentException)
        {
            whileCheckFunc = () => { return false; };
        }

        while (whileCheckFunc() || HasUpdatingChunks)
        {
            yield return null;
        }

        CallOnLevelMeshGenerated();

        yield break;
    }

    private void CallOnLevelMeshGenerated()
    {
        if (OnLevelMeshGenerated != null)
        {
            Debug.Log($"Generated the {this.transform.parent.name}.");
            OnLevelMeshGenerated();
        }
    }

    /// <param name="grids">Grids are ordered for x then z.</param>
    /// <param name="size"></param>
    public void GenerateAll(Grid[] grids, Vector3Int size, bool multithreaded = true, Task[] tasks = null)
    {
        if (tasks == null)
        {
            int len = size.x * size.z;
            tasks = new Task[len];

            for (int i = 0; i < len; i++)
            {
                tasks[i] = new Task(async () => { await Task.CompletedTask; });
            }
        }

        Clear();

        Debug.Assert(grids.Length > 0);
        Vector3 meshSizeInMeters = MeshDimensionsInMeters;

        ChunkSizeInVoxels = grids[0].Size;
        VoxelSizeInMeters = new Vector3(meshSizeInMeters.x / size.x, meshSizeInMeters.y / size.y, meshSizeInMeters.z / size.z);

        int index = 0;
        for (int x = 0; x < LevelSizeInChunks.x; x++)
        {
            for (int z = 0; z < LevelSizeInChunks.z; z++)
            {
                Vector3Int chunkAddress = new Vector3Int(x, 0, z);
                ChunkMeshController chunkController = InitiateChunk(chunkAddress);

                chunkController.Generate(grids[index], multithreaded, tasks[index]);
                index++;
            }
        }
    }


    private void InitiateChunkGrid()
    {
        _chunkGrid = new ChunkMeshController[LevelSizeInChunks.x, LevelSizeInChunks.y, LevelSizeInChunks.z];
    }

    private ChunkMeshController InitiateChunk(Vector3Int chunkAddress)
    {
        Vector3 chunkSizeInMeters = ChunkSizeInMeters;

        //GameObject chunkObject = new GameObject($"Chunk {chunkAddress.x} {chunkAddress.y} {chunkAddress.z}");
        //ChunkMeshController chunkController = chunkObject.AddComponent<ChunkMeshControllerSmooth>();
        //chunkController.SetLayer(layerOfMeshes);
        //chunkObject.transform.parent = _generatedJunksParent;

        GameObject chunkObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, _generatedJunksParent);
        chunkObject.transform.localPosition = Vector3.Scale(chunkAddress, chunkSizeInMeters);
        chunkObject.name = $"Chunk {chunkAddress.x} {chunkAddress.y} {chunkAddress.z}";
        ChunkMeshController chunkController = chunkObject.GetComponent<ChunkMeshControllerSmooth>();

        chunkController.levelMeshController = this;
        chunkController.SetChunkAddress(chunkAddress);
        chunkController.SetChunkSizeInMeters(chunkSizeInMeters);

        _chunkGrid[chunkAddress.x, chunkAddress.y, chunkAddress.z] = chunkController;

        if (OnChunkCreated != null)
        {
            OnChunkCreated(chunkController, chunkAddress);
        }

        return chunkController;
    }

    private void Clear()
    {
        ChunkSizeInVoxels = Vector3Int.zero;

        ChunkMeshController[] chunks = Chunks;
        foreach (ChunkMeshController chunk in chunks)
        {
            Destroy(chunk.gameObject);
        }

        InitiateChunkGrid();
    }

    /// <summary>
    /// Updates all meshes on the main thread in single frame. Dangerous, creates a huge spike.
    /// </summary>
    public void UpdateMeshes()
    {
        ChunkMeshController[] chunks = Chunks;
        for (int i = 0; i < chunks.Length; i++)
        {
            ChunkMeshController chunk = chunks[i];
            UpdateMesh(chunk);
        }

        CallOnLevelMeshGenerated();
    }

    public void UpdateMesh(ChunkMeshController chunk)
    {
        if (chunk.MarkMeshToUpdate())
        {
            lock (_updatingChunks)
            {
                _updatingChunks.Add(chunk);
            }
        }
    }

    public void OnChunkMeshUpdated(ChunkMeshController chunkMesh)
    {
        _updatedMeshCountOnCurrentFrame += 1;
        lock (_updatingChunks)
        {
            _updatingChunks.Remove(chunkMesh);
        }
    }

    public bool CheckUpdateMesh(ChunkMeshController chunkMesh)
    {
        return _updatedMeshCountOnCurrentFrame < updateMeshCountPerFrame;
    }


    public Vector3Int GetGridAddressOfWorldPos(Vector3 worldPos)
    {
        Vector3 realWorldPos = worldPos/* + GetHalfHorizontalMeshSizeInMeters()*/;
        int x = Mathf.FloorToInt(realWorldPos.x / VoxelSizeInMeters.x);
        int y = Mathf.FloorToInt(realWorldPos.y / VoxelSizeInMeters.y);
        int z = Mathf.FloorToInt(realWorldPos.z / VoxelSizeInMeters.z);
        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Calculates horizontal mesh size in meters. Returns 0 as y axis.
    /// </summary>
    /// <returns>Half of the mesh size.</returns>
    private Vector3 GetHalfHorizontalMeshSizeInMeters()
    {
        Vector3 halfSize = MeshDimensionsInMeters / 2f;
        return new Vector3(halfSize.x, 0f, halfSize.z);
    }


    private ChunkMeshController[] Chunks
    {
        get
        {
            List<ChunkMeshController> chunks = new List<ChunkMeshController>();

            if (_chunkGrid != null)
            {
                for (int x = 0; x < LevelSizeInChunks.x; x++)
                {
                    for (int y = 0; y < LevelSizeInChunks.y; y++)
                    {
                        for (int z = 0; z < LevelSizeInChunks.z; z++)
                        {
                            if (_chunkGrid[x, y, z] != null)
                            {
                                chunks.Add(_chunkGrid[x, y, z]);
                            }
                        }
                    }
                }
            }

            return chunks.ToArray();
        }
    }

    private Vector3 ChunkSizeInMeters
    {
        get
        {
            Vector3 meshSizeInMeters = MeshDimensionsInMeters;
            return new Vector3(meshSizeInMeters.x / LevelSizeInChunks.x, meshSizeInMeters.y / LevelSizeInChunks.y, meshSizeInMeters.z / LevelSizeInChunks.z);
        }
    }

    public delegate void GenerationCompletion();
    public delegate void ChunkCreation(ChunkMeshController chunkMeshController, Vector3Int chunkAddress);

    public LevelGeneratorBase LevelGenerator => _levelGenerator;
}
