using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/SpawnEffect", order = 1)]
public class Effect : ScriptableObject
{
    public List<Upgrade> Upgrades;
    public void OnEffectPickup(Character character)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            foreach(CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                character.ChangeCharacterStat(((int)statChange.StatEnum), statChange.Value);
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                character.ChangeWeaponStat(((int)statChange.StatEnum), statChange.Value);
            }
        }     
    }

    public void OnEffectDrop(Character character)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            foreach (CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                character.ChangeCharacterStat(((int)statChange.StatEnum), 
                    -statChange.Value);
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                character.ChangeWeaponStat(((int)statChange.StatEnum), 
                    -statChange.Value);
            }
        }
    }
}
