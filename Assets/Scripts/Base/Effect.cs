using Cyan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/SpawnEffect", order = 1)]
public class Effect : ScriptableObject
{
    public List<Upgrade> Upgrades;
    public Blit ScreenEffect;
    public bool InAnimation = false;
    public void OnEffectPickup(Character character)
    {
        character.ChangeEffect(this);
        ApplyUpgrades(character);
    }

    public void OnEffectStart()
    {
        StartScreenFX(ScreenEffect);
    }

    public void OnEffectEnd()
    {
        EndScreenFX(ScreenEffect);
    }
    public void OnEffectDrop(Character character)
    {
        RemoveUpgrades(character);
    }

    private void ApplyUpgrades(Character character)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            foreach (CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                character.ChangeCharacterStat(((int)statChange.StatEnum), statChange.Value);
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                character.ChangeWeaponStat(((int)statChange.StatEnum), statChange.Value);
            }
        }
    }

    private void RemoveUpgrades(Character character)
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

    private void StartScreenFX(Blit screenFX)
    {
        Debug.Log("StartScreenFX");

        screenFX.SetActive(true);
        Material blitMat = screenFX.settings.blitMaterial;
        Debug.Log(blitMat.GetFloat("_Intensity"));
        InAnimation = true;
        LeanTween.value(0, 1, 1.5f)
            .setEase(LeanTweenType.easeInQuint)
            .setOnUpdate(l =>
            {
                blitMat.SetFloat("_Intensity", l);
            })
            .setOnComplete(l => 
            {
                InAnimation = false;
            });
    }
    private void EndScreenFX(Blit screenFX)
    {
        Debug.Log("EndScreenFX");
        Material blitMat = screenFX.settings.blitMaterial;
        Debug.Log(blitMat.GetFloat("_Intensity"));
        InAnimation = true;
        LeanTween.value(1, 0, 1.5f)
            .setEase(LeanTweenType.easeOutQuint)
            .setOnUpdate(l =>
            {
                blitMat.SetFloat("_Intensity", l);
            })
            .setOnComplete(l =>
            {
                InAnimation = false;
                screenFX.SetActive(false);
            });
    }
}
