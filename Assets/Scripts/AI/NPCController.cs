using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Character), typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    private NavMeshAgent _agent;

    [HideInInspector]
    public Character character;

    private Character _targetCharacter;
    private float _targetCharacterPositionUpdated;

    private const float targetPosUpdateInterval = 0.5f;

    private void Awake()
    {
        character = GetComponent<Character>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Character character)
    {
        _targetCharacter = character;
        _targetCharacterPositionUpdated = Time.realtimeSinceStartup - targetPosUpdateInterval;
    }

    public bool SetTarget(Vector3 position)
    {
        _targetCharacter = null;
        return SetTargetInternal(position);
    }

    public bool SetRelativeTarget(Vector3 globalVector)
    {
        return SetTarget(transform.position + globalVector);
    }

    private bool SetTargetInternal(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 100, -1))
        {
            _agent.SetDestination(hit.position);
            return true;
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_targetCharacter != null && Time.realtimeSinceStartup - _targetCharacterPositionUpdated >= targetPosUpdateInterval)
        {
            SetTargetInternal(_targetCharacter.transform.position);
            _targetCharacterPositionUpdated = Time.realtimeSinceStartup;
        }
    }

    public void Stop()
    {
        _targetCharacter = null;
        _agent.ResetPath();
    }

    public NavMeshAgent Agent => _agent;
    public bool IsStopped => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
}
