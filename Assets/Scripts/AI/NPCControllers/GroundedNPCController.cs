using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class GroundedNPCController : NPCControllerBase
{
    private CharacterController _characterController;
    private CharacterMovement _cm;

    public bool rotateBodyTowardsMovement;

    public float shootingErrorRateToDistance;

    private Vector3 _currentTargetPosition;

    private bool _isStopped;

    public event StoppingHandler OnReached;

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
            MoveCharacterTowardsPosition(_currentTargetPosition);
            CheckIfReached(_currentTargetPosition);
        }

        if (rotateBodyTowardsMovement)
        {
            RotateBodyTowardsPosition(_currentTargetPosition);
        }

        if (CheckIfReached(_currentTargetPosition))
        {
            _isStopped = true;
            if (OnReached != null)
            {
                OnReached();
            }
        }

        if (DebugManager.instance.debugAi)
        {
            Debug.DrawLine(transform.position, _currentTargetPosition);
        }
    }

    public override bool IsStopped()
    {
        return _isStopped;
    }

    public override bool MoveTo(Vector3 position)
    {
        bool completed = base.MoveTo(position);

        if (completed)
        {
            _currentTargetPosition = position;
            _isStopped = position == transform.position;
        }

        return completed;
    }

    public override bool TryAttack(Character character)
    {
        bool shot = false;
        Weapon[] weapons = GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            shot |= weapon.TryShoot(character.transform.position);
        }
        return shot;
        //cagirildiginda bized dogru hucum edecek
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

    private void MoveCharacterTowardsPosition(Vector3 position)
    {
        bool moved = _cm.GroundedMoveTowards(position);
        //grounded yerine movetow

        if (!moved)
        {
            _isStopped = true;
        }
    }

    public override bool CheckIfReached(Vector3 targetPosition)
    {
        return new Vector3(transform.position.x - targetPosition.x, 0f, transform.position.z - targetPosition.z).sqrMagnitude <= reachCheckSqr;
        //burdaki y deðerini kaldýrsak bu sorun çözülmez mi drone için
        //0f yerine ysini hesaplat
    }

    public CharacterMovement CharacterMovement => _cm;
    public Vector3 Velocity => _characterController.velocity;

    public override Vector3 CurrentTargetPosition => _currentTargetPosition;

    public delegate void StoppingHandler();
}
