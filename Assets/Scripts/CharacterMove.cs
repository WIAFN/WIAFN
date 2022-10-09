using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    //Relations
    [Header("Relations")]
    public Camera fpsCam;
    public CharacterController controller;
    public PlayerAttributes attributes;
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
    Vector3 velocity;
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

    [Header("Movement Checks")]
    //Movement Checks
    public bool isMoving;
    public bool isGrounded;
    public bool isDashing;
    public bool isSprinting;

    void Start()
    {
        remainingJump = attributes.multiJumps;
        remainingDashes = attributes.multiDashes;
        dashDuration = attributes.dashDuration;
    }

    // Update is called once per frame
    void Update()
    {
        GravityCheck();

        PlayerCooldowns();

        MovePlayer();

        PlayerActions();

        if (isDashing)
        {
            Dashing();
        }

        if (isSprinting)
        {
            Sprinting();
        }
        if (speed != attributes.speed && !isDashing && !isSprinting)
        {
            SlowDown();
        }

        if (!isSprinting && !isDashing)
        {
            if(attributes.stamina < attributes.maxStamina)
            {
                attributes.RegenStamina();
            }         
        }
        ApplyGravity();
    }

    private void PlayerCooldowns()
    {
        if(remainingDashes < attributes.multiDashes)
        {
            dashCooldown -= Time.deltaTime;
        }

        if(dashCooldown <= 0)
        {
            dashCooldown = attributes.dashCooldown;
            remainingDashes++;
        }
    }
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void PlayerActions()
    {
        if (Input.GetButtonDown("Jump") && (isGrounded || remainingJump > 0))
        {
            velocity.y = Mathf.Sqrt(attributes.jumpHeight * -2f * gravity);
            remainingJump--;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && remainingDashes > 0 && attributes.stamina > attributes.dashCost && isMoving)
        {
            isDashing = true;
            remainingDashes--;
            attributes.stamina -= attributes.dashCost;
        }
        if(Input.GetKey(KeyCode.LeftControl) && isMoving)
        {
            isSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl) || !isMoving)
        {
            isSprinting = false;
            sprintDuration = 0;
            slowDownDuration = 0;
        }
    }

    private void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        isMoving = x != 0 || z != 0;

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(speed * Time.deltaTime * move);
    }

    private void GravityCheck()
    {
        isGrounded = Physics.CheckSphere(feet.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -10f;
            remainingJump = attributes.multiJumps;
        }
    }

    private void Dashing()
    {
        if (dashDuration < 0)
        {
            isDashing = false;
            dashDuration = attributes.dashDuration;
            dashSpeed = 0;
        }
        else
        {
            float percent = Mathf.InverseLerp(0, attributes.dashDuration, dashDuration);
            float curvePercent = dashCurve.Evaluate(percent);
            Debug.Log(percent + " - " + curvePercent);
            dashSpeed = Mathf.Lerp(0, attributes.dashSpeed, curvePercent);
            Vector3 move = fpsCam.transform.forward;
            controller.Move(dashSpeed * Time.deltaTime * move);
            dashDuration -= Time.deltaTime;
        }
    }

    private void Sprinting()
    {
        if(attributes.stamina > sprintStamina)
        {
            float percentage = Mathf.InverseLerp(0, sprintMaxSpeedTime, sprintDuration);
            speed = Mathf.SmoothStep(speed,attributes.sprintSpeed, percentage);
            attributes.stamina -= sprintStamina * Time.deltaTime;
            sprintDuration += Time.deltaTime;
        }
    }

    private void SlowDown()
    {
        if (speed > attributes.speed)
        {
            float percentage = Mathf.InverseLerp(0, sprintMaxSpeedTime, slowDownDuration);
            speed = Mathf.SmoothStep(attributes.sprintSpeed, attributes.speed, percentage);
            slowDownDuration += Time.deltaTime;
            slowDownDuration = Mathf.Clamp(slowDownDuration, 0, sprintMaxSpeedTime);
        }
    }
}
