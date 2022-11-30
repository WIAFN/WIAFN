using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor.AI;
using UnityEngine;

public class UpdateNavMeshScript : MonoBehaviour
{
    public float updateInterval;

    private bool _initialized;
    private NavMeshSurface _navMeshSurface;
    private float _lastUpdateTime;

    private AsyncOperation _lastUpdateOperation;

    // Start is called before the first frame update
    void Start()
    {
        _initialized = false;

        _navMeshSurface = GetComponent<NavMeshSurface>();
        StartCoroutine(BuildNavMeshNextFrame());
    }

    private IEnumerator BuildNavMeshNextFrame()
    {
        yield return new WaitForSeconds(0.5f);

        BuildNavMesh();
        _lastUpdateTime = Time.realtimeSinceStartup;
        _initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(_initialized && Time.realtimeSinceStartup - _lastUpdateTime >= updateInterval)
        {
            UpdateNavMesh();
        }
    }

    // Should only be called on initialization.
    private void BuildNavMesh()
    {
        _navMeshSurface.BuildNavMesh();
    }

    private void UpdateNavMesh()
    {
        var oldData = _navMeshSurface.navMeshData;

        if (_lastUpdateOperation == null || _lastUpdateOperation.isDone)
        {
            _lastUpdateOperation = _navMeshSurface.UpdateNavMesh(oldData);
            _lastUpdateTime = Time.realtimeSinceStartup;
        }
    }
}
