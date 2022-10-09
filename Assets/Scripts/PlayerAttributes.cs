using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    [Header("HP and Stamina")]
    public float health;
    public float stamina;
    public float maxStamina;
    public float maxHealth;
    [Header("Regenerations")]
    public float staminaRegen;
    public float healthRegen;

    [Header("Attributes")]
    public float jumpHeight = 4f;
    public int multiJumps = 2;
    public float speed = 12f;
    public float sprintSpeed = 30f;
    public float dashSpeed = 300f;
    public float multiDashes = 2f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 5f;
    public float dashCost = 40f;


    public void RegenStamina()
    {
        stamina += staminaRegen * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }
}
