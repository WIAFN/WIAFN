using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

[RequireComponent(typeof(LevelGeneratorBase))]
public class LevelMeshController : MonoBehaviour
{
    // Y is constrained to 1 for multithreaded generation.
    [SerializeField]
    private Vector3Int _levelSizeInChunks;

    //public Material terrainMaterial;

    public GameObject chunkPrefab;

    [Header("Parameters")]
    public int updateMeshCountPerFrame;
    public int frameSkipsBetweenMeshGenerations;

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

    public Vector3 MeshSizeInMeters
    {
        get
        {
            return _levelGenerator.levelSizeInMeters/* * 4f / 3f*/;
        }
    }

    private int _chunkCount;
    public bool HasUpdatingChunks { get { return _updatingChunks.Count > 0; } }

    private LevelGeneratorBase _levelGenerator;
    private ChunkMeshController[,,] _chunkGrid;

    private Transform _generatedJunksParent;

    private int _updatedMeshCountOnCurrentFrame;

    private HashSet<ChunkMeshController> _updatingChunks;

    private void Awake()
    {
        _levelGenerator = GetComponent<LevelGeneratorBase>();

        _generatedJunksParent = (new GameObject("Junks")).transform;
        _generatedJunksParent.parent = transform.parent;

        _updatingChunks = new HashSet<ChunkMeshController>();
    }

    private void Start()
    {
        Random.InitState(1);
        ResetUpdatedMeshOnFrame();
        _chunkCount = CalculateChunkCount();
    }

    private int CalculateChunkCount()
    {
        return LevelSizeInChunks.x * LevelSizeInChunks.y * LevelSizeInChunks.z;
    }

    private void LateUpdate()
    {
        ResetUpdatedMeshOnFrame();
    }

    private void ResetUpdatedMeshOnFrame()
    {
        _updatedMeshCountOnCurrentFrame = 0;
    }

    public void Generate(Grid grid, bool multithreaded = true)
    {
        Clear();

        Vector3 meshSizeInMeters = MeshSizeInMeters;
        Vector3 halfSize = meshSizeInMeters / 2f;
        _generatedJunksParent.localPosition = new Vector3(-halfSize.x, 0f, -halfSize.z);

        ChunkSizeInVoxels = new Vector3Int(grid.Size.x / LevelSizeInChunks.x, grid.Size.y / LevelSizeInChunks.y, grid.Size.z / LevelSizeInChunks.z);
        VoxelSizeInMeters = new Vector3(meshSizeInMeters.x / grid.Size.x, meshSizeInMeters.y / grid.Size.y, meshSizeInMeters.z / grid.Size.z);
        StartCoroutine(GenerateChunks(grid, multithreaded));
    }

    private IEnumerator GenerateChunks(Grid grid, bool multithreaded)
    {
        List<Task> tasks = new List<Task>();
        List<ChunkMeshController> chunks = new List<ChunkMeshController>();

        for (int x = 0; x < LevelSizeInChunks.x; x++)
        {
            for (int y = 0; y < LevelSizeInChunks.y; y++)
            {
                for (int z = 0; z < LevelSizeInChunks.z; z++)
                {
                    for (int i = 0; i < frameSkipsBetweenMeshGenerations; i++)
                    {
                        yield return null;
                    }

                    Vector3Int chunkAddress = new Vector3Int(x, y, z);
                    ChunkMeshController chunkController = InitiateChunk(chunkAddress);

                    Vector3Int minAddress = Vector3Int.Scale(ChunkSizeInVoxels, chunkAddress);

                    Vector3Int maxAddress = Vector3Int.Scale(ChunkSizeInVoxels, chunkAddress + Vector3Int.one)/* - Vector3Int.one*/;
                    chunkController.Generate(grid.GetSubGrid(minAddress, maxAddress), multithreaded);
                    //chunks.Add(chunkController);
                    tasks.Add(chunkController.MeshGenerationTask);
                    chunkController.MeshGenerationTask.ContinueWith((previousTask) =>
                    {
                        UpdateMesh(chunkController);
                    });
                }
            }
        }

        Task allWaitTask = Task.WhenAll(tasks);
        Task anyWaitTask = Task.WhenAny(tasks);

        while (!allWaitTask.IsCompleted || HasUpdatingChunks)
        {
            //Task finishedTask = Task.WhenAny(tasks);
            //int indexOfFinishedTask = tasks.IndexOf(finishedTask);
            //ChunkMeshController chunkController = chunks[indexOfFinishedTask];
            //chunks.RemoveAt(indexOfFinishedTask);
            //tasks.RemoveAt(indexOfFinishedTask);

            //UpdateMesh(chunkController);
            //Debug.Log($"Bitti: {allWaitTask.IsCompleted} - Chunk Var: {HasUpdatingChunks}");
            //Debug.Log($"Chunk Sayýsý: {_updatingChunks.Count}");
            yield return null;
        }

        if (OnLevelMeshGenerated != null)
        {
            Debug.Log($"Generated the {this.transform.parent.name}.");
            OnLevelMeshGenerated();
        }

        yield break;
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
        Vector3 meshSizeInMeters = MeshSizeInMeters;

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


    public void UpdateMeshes()
    {
        ChunkMeshController[] chunks = Chunks;
        for (int i = 0; i < chunks.Length; i++)
        {
            ChunkMeshController chunk = chunks[i];
            UpdateMesh(chunk);
        }

        if (OnLevelMeshGenerated != null)
        {
            OnLevelMeshGenerated();
        }
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
        int x = Mathf.FloorToInt(worldPos.x / VoxelSizeInMeters.x);
        int y = Mathf.FloorToInt(worldPos.y / VoxelSizeInMeters.y);
        int z = Mathf.FloorToInt(worldPos.z / VoxelSizeInMeters.z);
        return new Vector3Int(x, y, z);
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
                            chunks.Add(_chunkGrid[x, y, z]);
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
            Vector3 meshSizeInMeters = MeshSizeInMeters;
            return new Vector3(meshSizeInMeters.x / LevelSizeInChunks.x, meshSizeInMeters.y / LevelSizeInChunks.y, meshSizeInMeters.z / LevelSizeInChunks.z);
        }
    }

    public delegate void GenerationCompletion();
    public delegate void ChunkCreation(ChunkMeshController chunkMeshController, Vector3Int chunkAddress);

    public LevelGeneratorBase LevelGenerator => _levelGenerator;
}
