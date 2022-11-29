using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class GroundedNPCController : NPCControllerBase
{
    private CharacterController _characterController;
    private CharacterMovement _cm;

    public bool lookAtTheMovementDirection;

    private Vector3 _currentTargetPosition;

    private bool _isStopped;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        _isStopped = true;

        _characterController = GetComponent<CharacterController>();
        _cm = GetComponent<CharacterMovement>();

        _currentTargetPosition = transform.position;
    }

    private void OnDestroy()
    {
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (!_isStopped)
        {
            MoveTowardsTargetPosition();
        }

        if (lookAtTheMovementDirection)
        {
            LookAt(_currentTargetPosition, true);
        }
    }

    public override bool IsStopped()
    {
        return _isStopped;
    }

    public override bool MoveTo(Vector3 position)
    {
        _currentTargetPosition = position;
        _isStopped = position == transform.position;
        return true;
    }

    public override bool TryAttack(Character character)
    {
        return false;
        //throw new System.NotImplementedException();
    }

    public override bool StopMoving()
    {
        bool baseCompleted = base.StopMoving();
        if (baseCompleted)
        {
            _currentTargetPosition = transform.position;
            _isStopped = true;
        }
        return _isStopped;
    }

    private void MoveTowardsTargetPosition()
    {
        bool moved = _cm.GroundedMoveTowards(_currentTargetPosition);

        if (!moved)
        {
            _isStopped = true;
        }
    }

    public CharacterMovement CharacterMovement => _cm;
    public Vector3 Velocity => _characterController.velocity;
}
