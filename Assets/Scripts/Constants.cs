using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.Constants
{
    public class WIAFNTags
    {
        public static string Cross = "Cross";
    }

    public class WIAFNAnimatorParams
    {
        public static string Attacked = "Attacked";
    }

    public enum CharacterStats
    {
        maxHealth = 0,
        maxStamina = 1,
        healthRegen = 2,
        staminaRegen = 3,
        jumpHeight = 4,
        multiJumps = 5,
        defaultSpeed = 6,
        defaultSprintSpeed = 7,
        defaultDashSpeed = 8,
        multiDashes = 9,
        dashDuration = 10,
        dashCooldown = 11,
        dashCost = 12,
        speedCoefficient = 13,
    }

    public enum WeaponStats
    {
        Damage = 0,
        FireRate = 1,
    }
}