using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

[RequireComponent(typeof(LevelGeneratorBase))]
public class LevelMeshController : MonoBehaviour
{
    public Vector3Int levelSizeInChunks;

    public Material terrainMaterial;

    public delegate void ChunkCreation(ChunkMeshController chunkMeshController, Vector3Int chunkAddress);

    public event ChunkCreation OnChunkCreated;

    public ChunkMeshController[,,] ChunkGrid
    { 
        get
        {
            return _chunkGrid;
        }
    }

    public Vector3Int ChunkSizeInVoxels { get; private set; }
    public Vector3 VoxelSizeInMeters { get; private set; }

    public int layerOfMeshes;

    public Vector3 MeshSizeInMeters
    {
        get
        {
            return _levelGenerator.levelSizeInMeters * 4f / 3f;
        }
    }

    private LevelGeneratorBase _levelGenerator;
    private ChunkMeshController[,,] _chunkGrid;

    private Transform _generatedJunksParent;

    private void Awake()
    {
        _levelGenerator = GetComponent<LevelGeneratorBase>();

        _generatedJunksParent = (new GameObject("Junks")).transform;
        _generatedJunksParent.parent = transform.parent;
    }

    private void Start()
    {
        Random.InitState(1);
    }

    public void Generate(Grid grid)
    {
        Clear();

        Vector3 meshSizeInMeters = MeshSizeInMeters;

        ChunkSizeInVoxels = new Vector3Int(grid.Size.x / levelSizeInChunks.x, grid.Size.y / levelSizeInChunks.y, grid.Size.z / levelSizeInChunks.z);
        VoxelSizeInMeters = new Vector3(meshSizeInMeters.x / grid.Size.x, meshSizeInMeters.y / grid.Size.y, meshSizeInMeters.z / grid.Size.z);
        for (int x = 0; x < levelSizeInChunks.x; x++)
        {
            for (int y = 0; y < levelSizeInChunks.y; y++)
            {
                for (int z = 0; z < levelSizeInChunks.z; z++)
                {
                    Vector3Int chunkAddress = new Vector3Int(x, y, z);
                    ChunkMeshController chunkController = InitiateChunk(chunkAddress);

                    Vector3Int minAddress = Vector3Int.Scale(ChunkSizeInVoxels, chunkAddress);
                    
                    Vector3Int maxAddress = Vector3Int.Scale(ChunkSizeInVoxels, chunkAddress + Vector3Int.one) - Vector3Int.one;
                    chunkController.Generate(grid.GetSubGrid(minAddress, maxAddress));
                }
            }
        }
    }


    private void InitiateChunkGrid()
    {
        _chunkGrid = new ChunkMeshController[levelSizeInChunks.x, levelSizeInChunks.y, levelSizeInChunks.z];
    }

    private ChunkMeshController InitiateChunk(Vector3Int chunkAddress)
    {
        GameObject chunkObject = new GameObject(string.Format("Chunk {0} {1} {2}", chunkAddress.x, chunkAddress.y, chunkAddress.z));
        
        ChunkMeshController chunkController = chunkObject.AddComponent<ChunkMeshControllerSmooth>();
        chunkController.SetChunkAddress(chunkAddress);
        chunkController.SetLayer(layerOfMeshes);

        Vector3 chunkSizeInMeters = ChunkSizeInMeters;
        chunkController.SetChunkSizeInMeters(chunkSizeInMeters);

        chunkController.levelMeshController = this;
        chunkObject.transform.parent = _generatedJunksParent;
        chunkObject.transform.position = Vector3.Scale(chunkAddress, chunkSizeInMeters);

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
        Vector3 halfSize = MeshSizeInMeters / 2f;
        _generatedJunksParent.localPosition = new Vector3(-halfSize.x, 0f, -halfSize.z);

        foreach (ChunkMeshController chunk in Chunks)
        {
            chunk.UpdateMesh();
        }
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
                for (int x = 0; x < levelSizeInChunks.x; x++)
                {
                    for (int y = 0; y < levelSizeInChunks.y; y++)
                    {
                        for (int z = 0; z < levelSizeInChunks.z; z++)
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
            return new Vector3(meshSizeInMeters.x / levelSizeInChunks.x, meshSizeInMeters.y / levelSizeInChunks.y, meshSizeInMeters.z / levelSizeInChunks.z);
        }
    }

    public LevelGeneratorBase LevelGenerator => _levelGenerator;
}
