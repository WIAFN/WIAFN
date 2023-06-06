using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIAFN.Constants
{
    public class WIAFNScenes
    {
        public static string MainMenu = "Start";
        public static string TestScene = "PlayerTest";
        public static string Loading = "Loading Screen";
        public static int Entrance = 3;
        public static int Level0 = 4;
    }

    public class WIAFNTags
    {
        public static string Cross = "Cross";
    }

    public class WIAFNAnimatorParams
    {
        public static string Attacked = "Attacked";
        public static string Speed = "Speed";
        public static string InFight = "InFight";
        public static string Running = "Running";
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