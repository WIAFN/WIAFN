using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(LevelMeshController))]
public class ChunkedNavMeshGenerator : NavMeshGeneratorBase
{
    public float distantChunksUpdateInterval;
    public int chunkDistanceToUpdateSlower;

    private LevelMeshController _levelMeshController;
    private ChunkNavMeshController[,,] _chunkNavMeshControllers;

    private float _nextCloseChunksUpdate;
    private float _nextDistantChunksUpdate;

    private List<ChunkNavMeshController> _distantChunksToUpdate;

    // Start is called before the first frame update
    void Start()
    {
        _distantChunksToUpdate = new List<ChunkNavMeshController>();

        _levelMeshController = GetComponent<LevelMeshController>();
        Vector3Int levelSizeInChunks = _levelMeshController.levelSizeInChunks;
        _chunkNavMeshControllers = new ChunkNavMeshController[levelSizeInChunks.x, levelSizeInChunks.y, levelSizeInChunks.z];

        _levelMeshController.OnChunkCreated += OnChunkCreated;

        _nextCloseChunksUpdate = Time.realtimeSinceStartup;
        _nextDistantChunksUpdate = Time.realtimeSinceStartup;

        StartCoroutine(UpdateDistantChunks());
    }

    private IEnumerator UpdateDistantChunks()
    {
        while (true)
        {
            yield return null;
            yield return null;

            if (_distantChunksToUpdate.Count > 0)
            {
                ChunkNavMeshController chunkNavMesh = _distantChunksToUpdate[0];
                _distantChunksToUpdate.RemoveAt(0);

                chunkNavMesh.UpdateNavMeshIfSuitable();
            }
        }
    }

    private void OnChunkCreated(ChunkMeshController chunkMeshController, Vector3Int chunkAddress)
    {
        GameObject chunkObject = chunkMeshController.gameObject;
        NavMeshSurface chunkMeshSurface = chunkObject.AddComponent<NavMeshSurface>();
        chunkMeshSurface.collectObjects = CollectObjects.Volume;
        chunkMeshSurface.size = chunkMeshController.ChunkSizeInMeters;
        chunkMeshSurface.layerMask = includeLayers;

        _chunkNavMeshControllers[chunkAddress.x, chunkAddress.y, chunkAddress.z] = chunkObject.AddComponent<ChunkNavMeshController>();
    }

    private void OnDestroy()
    {
        _levelMeshController.OnChunkCreated -= OnChunkCreated;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int playerChunk = _levelMeshController.GetGridAddressOfWorldPos(GameManager.instance.mainPlayer.transform.position);
        Vector3Int levelSizeInChunks = _levelMeshController.levelSizeInChunks;

        // Update close chunks.
        if (Time.realtimeSinceStartup - _nextCloseChunksUpdate > navMeshUpdateInterval)
        {
            for (int i = 0; i < levelSizeInChunks.x; i++)
            {
                for (int j = 0; j < levelSizeInChunks.y; j++)
                {
                    for (int k = 0; k < levelSizeInChunks.z; k++)
                    {
                        Vector3Int currentChunkAddress = new Vector3Int(i, j, k);
                        if (Vector3Int.Distance(playerChunk, currentChunkAddress) <= chunkDistanceToUpdateSlower)
                        {
                            ChunkNavMeshController chunkNavMesh = _chunkNavMeshControllers[i, j, k];
                            chunkNavMesh.UpdateNavMeshIfSuitable();
                            _distantChunksToUpdate.Remove(chunkNavMesh);
                        }
                    }
                }
            }

            _nextCloseChunksUpdate = Time.realtimeSinceStartup;
        }

        // Update distant chunks.
        if (Time.realtimeSinceStartup - _nextDistantChunksUpdate > distantChunksUpdateInterval)
        {
            for (int i = 0; i < levelSizeInChunks.x; i++)
            {
                for (int j = 0; j < levelSizeInChunks.y; j++)
                {
                    for (int k = 0; k < levelSizeInChunks.z; k++)
                    {
                        Vector3Int currentChunkAddress = new Vector3Int(i, j, k);
                        if (Vector3Int.Distance(playerChunk, currentChunkAddress) > chunkDistanceToUpdateSlower)
                        {
                            ChunkNavMeshController chunkNavMesh = _chunkNavMeshControllers[i, j, k];
                            //chunkNavMesh.UpdateNavMeshIfSuitable();
                            if (!_distantChunksToUpdate.Contains(chunkNavMesh))
                            {
                                _distantChunksToUpdate.Add(chunkNavMesh);
                            }
                        }
                    }
                }
            }

            _nextDistantChunksUpdate = Time.realtimeSinceStartup;
        }
    }
}
