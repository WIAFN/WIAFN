using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    private CharacterController _controller;
    private Character _character;
    private CharacterBaseStats _baseStats;

    //Relations
    [Header("Relations")]
    public Camera fpsCam;
    public Transform feet;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    //Jumping
    [Header("Jumping")]
    public float gravity = -9.8f;
    public float remainingJump;

    //Walking
    [Header("Speed")]
    public float speed = 12f;
    public Vector3 Velocity;
    //Sprinting
    public float sprintMaxSpeedTime = 0.5f;
    public float sprintDuration = 0f;
    public float slowDownDuration = 0f;
    public float sprintStamina = 10f;

    [Header("Dashing")]
    //Dashing
    public float remainingDashes;
    public float dashDuration;
    public float dashCooldown;
    public float dashSpeed;
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

        remainingJump = _baseStats.multiJumps;
        remainingDashes = _baseStats.multiDashes;
        dashDuration = _baseStats.dashDuration;
    }

    // Update is called once per frame
    void Update()
    {
        GravityCheck();

        PlayerCooldowns();

        MovePlayer();

        PlayerActions();

        if (IsDashing)
        {
            Dashing();
        }

        if (IsSprinting)
        {
            Sprinting();
        }
        if (speed != _baseStats.Speed && !IsDashing && !IsSprinting)
        {
            SlowDown();
        }
        ApplyGravity();
    }

    private void PlayerCooldowns()
    {
        if(remainingDashes < _baseStats.multiDashes)
        {
            dashCooldown -= Time.deltaTime;
        }

        if(dashCooldown <= 0)
        {
            dashCooldown = _baseStats.dashCooldown;
            remainingDashes++;
        }
    }
    private void ApplyGravity()
    {
        Velocity.y += gravity * Time.deltaTime;
        _controller.Move(Velocity * Time.deltaTime);
    }

    private void PlayerActions()
    {
        if (Input.GetButtonDown("Jump") && (IsGrounded || remainingJump > 0))
        {
            Velocity.y = Mathf.Sqrt(_baseStats.jumpHeight * -2f * gravity);
            remainingJump--;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && remainingDashes > 0 && _character.stamina > _baseStats.dashCost && IsMoving)
        {
            IsDashing = true;
            remainingDashes--;
            _character.stamina -= _baseStats.dashCost;
        }
        if(Input.GetKey(KeyCode.LeftControl) && IsMoving)
        {
            IsSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl) || !IsMoving)
        {
            IsSprinting = false;
            sprintDuration = 0;
            slowDownDuration = 0;
        }
    }

    private void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        IsMoving = x != 0 || z != 0;

        Vector3 move = transform.right * x + transform.forward * z;

        _controller.Move(speed * Time.deltaTime * move);
    }

    private void GravityCheck()
    {
        IsGrounded = Physics.CheckSphere(feet.position, groundDistance, groundMask);

        if (IsGrounded && Velocity.y < 0)
        {
            Velocity.y = -10f;
            remainingJump = _baseStats.multiJumps;
        }
    }

    private void Dashing()
    {
        if (dashDuration < 0)
        {
            IsDashing = false;
            dashDuration = _baseStats.dashDuration;
            dashSpeed = 0;
        }
        else
        {
            float percent = Mathf.InverseLerp(0, _baseStats.dashDuration, dashDuration);
            float curvePercent = dashCurve.Evaluate(percent);
            Debug.Log(percent + " - " + curvePercent);
            dashSpeed = Mathf.Lerp(0, _baseStats.defaultDashSpeed, curvePercent);
            Vector3 move = fpsCam.transform.forward;
            _controller.Move(dashSpeed * Time.deltaTime * move);
            dashDuration -= Time.deltaTime;
        }
    }

    private void Sprinting()
    {
        if(_character.stamina > sprintStamina)
        {
            float percentage = Mathf.InverseLerp(0, sprintMaxSpeedTime, sprintDuration);
            speed = Mathf.SmoothStep(speed,_baseStats.defaultSprintSpeed, percentage);
            _character.stamina -= sprintStamina * Time.deltaTime;
            sprintDuration += Time.deltaTime;
        }
    }

    private void SlowDown()
    {
        if (speed > _baseStats.defaultSpeed)
        {
            float percentage = Mathf.InverseLerp(0, sprintMaxSpeedTime, slowDownDuration);
            speed = Mathf.SmoothStep(_baseStats.defaultSprintSpeed, _baseStats.defaultSpeed, percentage);
            slowDownDuration += Time.deltaTime;
            slowDownDuration = Mathf.Clamp(slowDownDuration, 0, sprintMaxSpeedTime);
        }
    }
}
