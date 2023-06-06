using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    private CharacterController _controller;
    private Character _character;
    private CharacterBaseStats _baseStats;
    //public AudioClip walkingSound;

    //Relations
    // Ground check info if needed.
    [Header("Relations")]
    public Transform feet;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;
    [HideInInspector]
    public float gravityMultiplier;
    public bool hasGravity;
    //Jumping
    [Header("Jumping")]
    private float _remainingJump;
    
    //Walking
    [Header("Speed")]
    private float _targetSpeed;

    private Vector3 _velocity;
    private float _speed;
    private Vector3 _verticalVelocity;

    private float _lastSpeedCoefficient;

    //Sprinting
    public float sprintMaxSpeedTime = 0.5f;
    private float sprintDuration = 0f;
    private float slowDownDuration = 0f;
    public float sprintStamina = 10f;

    //Dashing
    [Header("Dashing")]
    private float _remainingDashes;
    private float _dashDuration;
    private float _dashCooldown;
    private float _dashCurrentSpeed;
    public AnimationCurve dashCurve;

    //Movement Checks
    public bool IsMoving { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSprinting { get; private set; }

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _character = GetComponent<Character>();
        _baseStats = _character.BaseStats;

        _remainingJump = _baseStats.multiJumps;
        _remainingDashes = _baseStats.multiDashes;
        _dashDuration = _baseStats.dashDuration;

        IsMoving = IsGrounded = IsDashing = IsSprinting = false;

        _lastSpeedCoefficient = -1f;

        gravityMultiplier = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        _speed = _controller.velocity.magnitude;
      
        _controller.Move(_velocity);
        _velocity = Vector3.zero;

        if (_lastSpeedCoefficient != _baseStats.speedCoefficient)
        {
            _targetSpeed = _baseStats.Speed;
            _lastSpeedCoefficient = _baseStats.speedCoefficient;
        }

        GravityCheck();

        CharacterCooldowns();

        CharacterActions();
        UpdateMovementSkills();

        if (_targetSpeed != _baseStats.Speed && !IsDashing && !IsSprinting)
        {
            SlowDown();
        }

        ApplyGravity();
    }

    private void UpdateMovementSkills()
    {
        if (IsDashing)
        {
            UpdateDashing();
        }

        if (IsSprinting)
        {
            UpdateSprinting();
        }
    }

    private void CharacterCooldowns()
    {
        if(_remainingDashes < _baseStats.multiDashes)
        {
            _dashCooldown -= Time.deltaTime;
        }

        if(_dashCooldown <= 0)
        {
            _dashCooldown = _baseStats.dashCooldown;
            _remainingDashes++;
        }
    }

    private void ApplyGravity()
    {
        if (hasGravity)
        {
            _verticalVelocity.y += AppliedGravity.y * Time.deltaTime;
            _velocity += _verticalVelocity * Time.deltaTime;
            //_controller.Move(_verticalVelocity * Time.deltaTime);
        }
    }

    private void CharacterActions()
    {
        if (!IsMoving)
        {
            StopSprinting();
        }
    }

    /// <summary>
    /// Moves the player.
    /// </summary>
    public bool MovePlayer(float x, float z)
    {
        Vector3 move = transform.right * x + transform.forward * z;
        return Move(move);
    }

    public bool GroundedMove(Vector3 direction)
    {
        Vector3 newDirection = new Vector3(direction.x, 0f, direction.z);
        return Move(newDirection);
    }

    public bool GroundedMoveTowards(Vector3 position)
    {
        return GroundedMove(Vector3.ClampMagnitude((position - transform.position), 1f));
    }

    public bool MoveTowards(Vector3 position)
    {
        return Move(Vector3.Normalize(position - transform.position));
    }

    /// <summary>
    /// Moves the character in the direction. Direction is usually a unit vector.
    /// </summary>
    /// <param name="weightedDirection">The direction to be moved.</param>
    public bool Move(Vector3 weightedDirection)
    {
        IsMoving = weightedDirection.x != 0f || weightedDirection.y != 0f || weightedDirection.z != 0f;
        _velocity += _targetSpeed * weightedDirection * Time.deltaTime;

        //AudioSource audioSource = GetComponent<AudioSource>();
        //audioSource.clip = walkingSound;
        //audioSource.Play();
        return IsMoving;
    }

    public bool StartSprinting()
    {
        if (IsMoving)
        {
            IsSprinting = true;
        }

        return IsSprinting;
    }

    public bool StopSprinting()
    {
        if (IsSprinting)
        {
            IsSprinting = false;
            sprintDuration = 0;
            slowDownDuration = 0;
        }

        return true;
    }

    public bool Dash(bool force = false)
    {
        if (force || (!IsDashing && _remainingDashes > 0 && _character.stamina > _baseStats.dashCost && IsMoving))
        {
            IsDashing = true;
            _remainingDashes--;
            _character.RemoveStamina(_baseStats.dashCost);
        }

        return IsDashing;
    }

    public bool Jump(bool force = false)
    {
        if (force || IsGrounded || _remainingJump > 0)
        {
            _verticalVelocity.y = Mathf.Sqrt(_baseStats.jumpHeight * -2f * AppliedGravity.y);
            _remainingJump--;

            return true;
        }

        return false;
    }

    private void GravityCheck()
    {
        if (!hasGravity)
        {
            return;
        }
        if (feet != null)
        {
            IsGrounded = Physics.CheckSphere(feet.position, groundDistance, groundMask);
        }
        else
        {
            IsGrounded = _controller.isGrounded;
        }

        if (IsGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -10f;
            _remainingJump = _baseStats.multiJumps;
        }
    }

    private void UpdateDashing()
    {
        if (_dashDuration < 0)
        {
            IsDashing = false;
            _dashDuration = _baseStats.dashDuration;
            _dashCurrentSpeed = 0;
        }
        else
        {
            float percent = Mathf.InverseLerp(0, _baseStats.dashDuration, _dashDuration);
            float curvePercent = dashCurve.Evaluate(percent);
            //Debug.Log(percent + " - " + curvePercent);
            _dashCurrentSpeed = Mathf.Lerp(0, _baseStats.defaultDashSpeed, curvePercent);
            Vector3 move = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            //_controller.Move(_dashCurrentSpeed * Time.deltaTime * move.normalized);
            _velocity += _dashCurrentSpeed * move.normalized * Time.deltaTime;
            _dashDuration -= Time.deltaTime;
        }
    }

    private void UpdateSprinting()
    {
        if(_character.stamina > sprintStamina)
        {
            float percentage = Mathf.InverseLerp(0, sprintMaxSpeedTime, sprintDuration);
            _targetSpeed = Mathf.SmoothStep(_baseStats.Speed, _baseStats.SprintSpeed, percentage);
            _character.RemoveStamina(sprintStamina * Time.deltaTime);
            sprintDuration += Time.deltaTime;
        }
    }

    private void SlowDown()
    {
        if (_targetSpeed > _baseStats.Speed)
        {
            float percentage = Mathf.InverseLerp(0, sprintMaxSpeedTime, slowDownDuration);
            _targetSpeed = Mathf.SmoothStep(_baseStats.SprintSpeed, _baseStats.Speed, percentage);
            slowDownDuration += Time.deltaTime;
            slowDownDuration = Mathf.Clamp(slowDownDuration, 0, sprintMaxSpeedTime);
        }
    }

    public float Speed => _speed;
    public Vector3 VerticalVelocity => _verticalVelocity;

    public Vector3 Velocity => _controller.velocity;

    public Vector3 AppliedGravity => Physics.gravity * gravityMultiplier;

    public bool IsStopped => !IsMoving;
}
