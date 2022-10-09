using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelMeshController : MonoBehaviour
{
    public Vector3 levelSizeInMeters;
    public Vector3Int levelSizeInChunks;

    public Material terrainMaterial;

    private ChunkMeshController[,,] chunkGrid;

    private void Start()
    {
        Random.InitState(1);
    }

    public void Generate(Grid grid)
    {
        ClearChunks();

        Vector3Int chunkSizeInVoxels = new Vector3Int(grid.Size.x / levelSizeInChunks.x, grid.Size.y / levelSizeInChunks.y, grid.Size.z / levelSizeInChunks.z);
        for (int x = 0; x < levelSizeInChunks.x; x++)
        {
            for (int y = 0; y < levelSizeInChunks.y; y++)
            {
                for (int z = 0; z < levelSizeInChunks.z; z++)
                {
                    Vector3Int chunkAddress = new Vector3Int(x, y, z);
                    ChunkMeshController chunkController = InitiateChunk(chunkAddress);

                    Vector3Int minAddress = Vector3Int.Scale(chunkSizeInVoxels, chunkAddress);
                    Vector3Int maxAddress = Vector3Int.Scale(chunkSizeInVoxels, chunkAddress + new Vector3Int(1, 1, 1)) - new Vector3Int(1, 1, 1);
                    chunkController.Generate(grid.GetSubGrid(minAddress, maxAddress));
                }
            }
        }
    }


    private void InitiateChunkGrid()
    {
        chunkGrid = new ChunkMeshController[levelSizeInChunks.x, levelSizeInChunks.y, levelSizeInChunks.z];
    }

    private ChunkMeshController InitiateChunk(Vector3Int chunkAddress)
    {
        GameObject chunkObject = new GameObject(string.Format("Chunk {0} {1} {2}", chunkAddress.x, chunkAddress.y, chunkAddress.z));
        
        ChunkMeshController chunkController = chunkObject.AddComponent<ChunkMeshControllerSmooth>();
        chunkController.SetChunkAddress(chunkAddress);
        Vector3 chunkSizeInMeters = new Vector3(levelSizeInMeters.x / levelSizeInChunks.x, levelSizeInMeters.y / levelSizeInChunks.y, levelSizeInMeters.z / levelSizeInChunks.z);
        chunkController.SetChunkSizeInMeters(chunkSizeInMeters);

        chunkController.levelMeshController = this;
        chunkObject.transform.parent = transform;
        chunkObject.transform.position = Vector3.Scale(chunkAddress, chunkSizeInMeters);

        chunkGrid[chunkAddress.x, chunkAddress.y, chunkAddress.z] = chunkController;
        return chunkController;
    }

    private void ClearChunks()
    {
        ChunkMeshController[] chunks = Chunks;
        foreach (ChunkMeshController chunk in chunks)
        {
            Destroy(chunk.gameObject);
        }

        InitiateChunkGrid();
    }


    public void UpdateMeshes()
    {
        foreach (ChunkMeshController chunk in Chunks)
        {
            chunk.UpdateMesh();
        }
    }

    private ChunkMeshController[] Chunks
    {
        get
        {
            List<ChunkMeshController> chunks = new List<ChunkMeshController>();

            if (chunkGrid != null)
            {
                for (int x = 0; x < levelSizeInChunks.x; x++)
                {
                    for (int y = 0; y < levelSizeInChunks.y; y++)
                    {
                        for (int z = 0; z < levelSizeInChunks.z; z++)
                        {
                            chunks.Add(chunkGrid[x, y, z]);
                        }
                    }
                }
            }

            return chunks.ToArray();
        }
    }
}
