using Cyan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/SpawnEffect", order = 1)]
public class Effect : ScriptableObject
{
    public List<Upgrade> Upgrades;
    public List<Upgrade> TempUpgrades;
    public Blit ScreenEffect;
    public bool InAnimation = false;
    public bool Enabled = false;
    public float EffectDuration;
    public float EffectCooldown;
    public bool IsUsedOnce;
    public int UseCount;
    private Character _owner;
    public void OnEffectPickup(Character character)
    {
        _owner = character;
        _owner.ChangeEffect(this);
        ApplyUpgrades();
    }

    public void OnEffectStart()
    {
        if (ScreenEffect != null)
        {
            StartScreenFX(ScreenEffect);
        }
    }

    public void OnEffectEnd()
    {
        if (ScreenEffect != null)
        {
            EndScreenFX(ScreenEffect);
        }
    }
    public void OnEffectDrop()
    {
        if(Enabled)
        {
            OnEffectEnd();
        }
        RemoveUpgrades();
    }

    private void ApplyUpgrades(bool isTemp = false)
    {
        List<Upgrade> effectives = this.Upgrades;
        if (isTemp)
        {
            effectives = this.TempUpgrades;
        }
        foreach (Upgrade upgrade in effectives)
        {
            foreach (CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                _owner.ChangeCharacterStat(((int)statChange.StatEnum), statChange.Value);
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                _owner.ChangeWeaponStat(((int)statChange.StatEnum), statChange.Value);
            }
        }
    }

    private void RemoveUpgrades(bool isTemp = false)
    {
        List<Upgrade> effectives = this.Upgrades;
        if (isTemp)
        {
            effectives = this.TempUpgrades;
        }
        foreach (Upgrade upgrade in effectives)
        {
            foreach (CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                _owner.ChangeCharacterStat(((int)statChange.StatEnum),
                    -statChange.Value);
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                _owner.ChangeWeaponStat(((int)statChange.StatEnum),
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
                ApplyUpgrades(true);
                Enabled = true;
                UseCount++;                
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
                RemoveUpgrades(true);
                Enabled = false;
            });
    }
}
