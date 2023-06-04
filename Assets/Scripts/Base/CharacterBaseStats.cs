using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class CharacterBaseStats : MonoBehaviour
{
    [Header("HP and Stamina")]
    public float maxHealth;
    public float maxStamina;
    [Header("Regenerations")]
    public float staminaRegen;
    public float healthRegen;

    [Header("Attributes")]
    public float jumpHeight = 4f;
    public int multiJumps = 2;

    public float defaultSpeed = 12f;
    public float defaultSprintSpeed = 30f;
    public float defaultDashSpeed = 300f;
    public int multiDashes = 2;
    public float dashDuration = 0.3f;
    public float dashCooldown = 5f;
    public float dashCost = 40f;

    public Vector2 shootingErrorRateMinMax;

    [Header("Runtime Variables")]
    public float speedCoefficient = 1f;

    public float Speed
    {
        get
        {
            return defaultSpeed * speedCoefficient;
        }
    }

    public float SprintSpeed
    {
        get
        {
            return defaultSprintSpeed * speedCoefficient;
        }
    }

    public float DashSpeed
    {
        get
        {
            return defaultDashSpeed * speedCoefficient;
        }
    }

    public float MaxRunSpeed
    {
        get
        {
            return Math.Max(defaultSpeed, defaultSprintSpeed);
        }
    }
}
