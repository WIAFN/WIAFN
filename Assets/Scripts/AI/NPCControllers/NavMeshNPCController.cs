using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshNPCController : NPCControllerBase
{
    private NavMeshAgent _agent;
   
//private bool _isOnOffMeshLink;
//private Object _onMeshLinkOfObject;

public override void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override bool MoveTo(Vector3 position)
    {
        base.MoveTo(position);

        return SetTargetInternal(position);
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
    public override void Update()
    {
        base.Update();

        _agent.speed = character.BaseStats.Speed;

        if (_agent.isOnOffMeshLink)
        {
            _agent.CompleteOffMeshLink();
        }


        //if (_agent.isOnOffMeshLink != _isOnOffMeshLink)
        //{
        //    _onMeshLinkOfObject = _agent.navMeshOwner;
        //    _agent.updatePosition = !_agent.isOnOffMeshLink;
        //    _agent.isStopped = _agent.isOnOffMeshLink;
        //    _isOnOffMeshLink = _agent.isOnOffMeshLink;
        //} else if (_onMeshLinkOfObject != null && _agent.navMeshOwner != _onMeshLinkOfObject)
        //{
        //    _onMeshLinkOfObject = null;
        //    _agent.updatePosition = true;
        //    _agent.isStopped = false;
        //    _isOnOffMeshLink = false;
        //    _agent.CompleteOffMeshLink();
        //}

        //if (_isOnOffMeshLink)
        //{
        //    Vector3 endPos = _agent.currentOffMeshLinkData.endPos;
        //    MoveManually(endPos);

        //    //if ((_agent.nextPosition - endPos).sqrMagnitude < 0.0001f)
        //    //{
        //    //    _agent.CompleteOffMeshLink();
        //    //}
        //}

        if (!_agent.isOnOffMeshLink && !_agent.isOnNavMesh)
        {
            Debug.LogError("NavMesh NPC " + gameObject.name + " is not on a navmesh.");
            gameObject.SetActive(false);
        }
    }

    public void MoveManually(Vector3 position)
    {
        //Vector3 diff = position - _agent.nextPosition;
        if (_agent.SamplePathPosition(_agent.areaMask, character.BaseStats.Speed, out NavMeshHit hit))
        {
            _agent.nextPosition = position;
            //_agent.nextPosition = Vector3.Lerp(_agent.nextPosition, position, Time.deltaTime *  character.BaseStats.Speed);
        }
    }

    public void Stop()
    {
        followCharacter = null;
        _agent.ResetPath();
    }

    public override bool TryAttack(Character character)
    {
        throw new System.NotImplementedException();
    }

    public override bool IsStopped()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }

    public NavMeshAgent Agent => _agent;
}
