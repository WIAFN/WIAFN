using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class ChunkNavMeshController : MonoBehaviour
{
    public NavMeshSurface NavMeshSurface => _navMeshSurface;

    private bool _initialized;
    private NavMeshSurface _navMeshSurface;
    private ChunkMeshController _chunkMeshController;

    private AsyncOperation _lastUpdateOperation;

    //private List<NavMeshLink> _navMeshLinks;

    private const float navMeshLinkOffsetMultiplier = 0.005f;

    // Start is called before the first frame update
    void Start()
    {
        _initialized = false;

        //_navMeshLinks = new List<NavMeshLink>();

        _navMeshSurface = GetComponent<NavMeshSurface>();
        _chunkMeshController = GetComponent<ChunkMeshController>();
        StartCoroutine(BuildNavMeshAfter());
    }

    private IEnumerator BuildNavMeshAfter()
    {
        yield return null;

        BuildNavMesh();
        BuildNavMeshOffLinks();
        _initialized = true;
    }

    // Should only be called on initialization.
    private void BuildNavMesh()
    {
        _navMeshSurface.BuildNavMesh();
    }

    public void UpdateNavMeshIfSuitable()
    {
        var oldData = _navMeshSurface.navMeshData;

        if (_initialized && oldData != null && (_lastUpdateOperation == null || _lastUpdateOperation.isDone))
        {
            _lastUpdateOperation = _navMeshSurface.UpdateNavMesh(oldData);
        }

        //foreach (NavMeshLink link in _navMeshLinks)
        //{
        //    link.UpdateLink();
        //}
    }

    public void ActivateNavMesh()
    {
        _navMeshSurface.AddData();
    }

    public void DeactivateNavMesh()
    {
        _navMeshSurface.RemoveData();
    }

    private void BuildNavMeshOffLinks()
    {

        Vector3Int[] neighbours = _chunkMeshController.GetNeighbours();
        Vector3 chunkSize = _chunkMeshController.ChunkSizeInMeters;
        Vector3 chunkHalfSize = chunkSize / 2f;

        foreach (var neighbour in neighbours)
        {
            Vector3Int neighbourDirection = neighbour - _chunkMeshController.ChunkAddress;

            if (neighbourDirection.sqrMagnitude == 1 && Math.Abs(neighbourDirection.y) == 1)
            {
                continue;
            }

            GameObject offMeshLinksObject = new GameObject($"OffMeshLinks ({neighbourDirection})");
            offMeshLinksObject.transform.parent = transform;
            Quaternion toRotate = Quaternion.FromToRotation(neighbourDirection, transform.forward);
            Vector3 rotatedChunkSize = toRotate * chunkSize;

            float absoluteChunkSizeX = Math.Abs(rotatedChunkSize.x);
            //float absoluteChunkSizeY = Math.Abs(rotatedChunkSize.y);

            NavMeshLink link = offMeshLinksObject.AddComponent<NavMeshLink>();
            Vector3 edgeRelativePosition = Vector3.Scale(neighbourDirection, chunkHalfSize);
            link.startPoint = transform.position + edgeRelativePosition * (1 - navMeshLinkOffsetMultiplier);
            link.endPoint = transform.position + edgeRelativePosition * (1 + navMeshLinkOffsetMultiplier);
            link.bidirectional = false;
            link.autoUpdate = false;
            link.width = absoluteChunkSizeX;
            link.autoUpdate = true;

            //_navMeshLinks.Add(link);
        }
    }

    public NavMeshData NavMeshData => _navMeshSurface.navMeshData;
}
