using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class ChunkNavMeshController : MonoBehaviour
{
    public NavMeshSurface NavMeshSurface => _navMeshSurface;

    private bool _initialized;
    private NavMeshSurface _navMeshSurface;

    private AsyncOperation _lastUpdateOperation;

    // Start is called before the first frame update
    void Start()
    {
        _initialized = false;

        _navMeshSurface = GetComponent<NavMeshSurface>();
        StartCoroutine(BuildNavMeshAfter());
    }

    private IEnumerator BuildNavMeshAfter()
    {
        yield return null;

        BuildNavMesh();
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
    }
}
