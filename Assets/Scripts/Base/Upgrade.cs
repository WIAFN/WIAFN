using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIAFN.Constants;

[System.Serializable]
public struct CharacterStatChange
{
    public CharacterStats StatEnum;
    public float Value;
}
[System.Serializable]
public struct WeaponStatChange
{
    public WeaponStats StatEnum;
    public float Value;
}


[CreateAssetMenu(fileName = "Upgrade", menuName = "ScriptableObjects/CreateUpgrade", order = 1)]
public class Upgrade:ScriptableObject
{
    public List<CharacterStatChange> CharacterStatChanges;
    public List<WeaponStatChange> WeaponStatChanges;
    public bool IsDetrimental = false;
    public float Multiplier = 1f;
    public float GetMultiplier()
    {
        return IsDetrimental ? -Multiplier : Multiplier;
    }
}
