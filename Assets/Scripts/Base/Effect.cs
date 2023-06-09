using Cyan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/SpawnEffect", order = 1)]
public class Effect : ScriptableObject
{
    [Header("For Simple Upgrades")]
    public List<Upgrade> Upgrades = new List<Upgrade>();
    [Header("For Effect")]
    public List<Upgrade> Boosts = new List<Upgrade>();
    public Blit ScreenEffect;
    public bool InAnimation = false;
    public bool Enabled = false;
    public float AnimationDuration;
    public float EffectDuration;
    public float EffectCooldown;
    public bool IsUsedOnce;
    public int UseCount;
    public LeanTweenType animationEaseIn;
    public LeanTweenType animationEaseOut;
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
            ApplyBoosts();
            StartScreenFX(ScreenEffect);
        }
    }

    public void OnEffectEnd()
    {
        if (ScreenEffect != null)
        {
            RemoveBoosts();
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

    private void ApplyUpgrades()
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            float multiplier = upgrade.GetMultiplier();
            foreach (CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                _owner.ChangeCharacterStat(((int)statChange.StatEnum), statChange.Value * multiplier);
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                _owner.ChangeWeaponStat(((int)statChange.StatEnum), statChange.Value * multiplier);
            }
        }
    }

    private void ApplyBoosts()
    {
        //TODO: Find a way to distinguish between boosted stats and base stats, probably need a base stats overhaul
        foreach (Upgrade boost in Boosts)
        {
            int statId;
            float currentValue;
            float calculatedValue;
            foreach (CharacterStatChange statChange in boost.CharacterStatChanges)
            {
                statId = (int)statChange.StatEnum;
                currentValue = _owner.GetCharacterStat(statId);
                calculatedValue = currentValue + statChange.Value;
                LeanTween.value(currentValue, calculatedValue, AnimationDuration)
                    .setEase(animationEaseIn)
                    .setOnUpdate( val =>
                    {
                        _owner.ChangeCharacterStat(statId, val);
                    });
            }

            foreach (WeaponStatChange statChange in boost.WeaponStatChanges)
            {
                statId = (int)statChange.StatEnum;
                currentValue = _owner.GetWeaponStat(statId);
                calculatedValue = currentValue + statChange.Value;
                Debug.Log(statId + " " + currentValue + " " + calculatedValue);
                LeanTween.value(currentValue, calculatedValue, AnimationDuration)
                    .setEase(animationEaseIn)
                    .setOnUpdate(val =>
                    {
                        _owner.ChangeWeaponStat(statId, val);
                    });
            }
        }
    }

    private void RemoveUpgrades()
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            float multiplier = upgrade.GetMultiplier();
            foreach (CharacterStatChange statChange in upgrade.CharacterStatChanges)
            {
                _owner.ChangeCharacterStat(((int)statChange.StatEnum),
                    -(statChange.Value * multiplier));
            }

            foreach (WeaponStatChange statChange in upgrade.WeaponStatChanges)
            {
                _owner.ChangeWeaponStat(((int)statChange.StatEnum),
                    -(statChange.Value * multiplier));
            }
        }
    }

    private void RemoveBoosts()
    {
        //TODO: Find a way to distinguish between boosted stats and base stats, probably need a base stats overhaul
        foreach (Upgrade boost in Boosts)
        {
            int statId;
            float currentValue;
            float calculatedValue;
            foreach (CharacterStatChange statChange in boost.CharacterStatChanges)
            {
                statId = (int)statChange.StatEnum;
                currentValue = _owner.GetCharacterStat(statId);
                calculatedValue = currentValue - statChange.Value;
                LeanTween.value(currentValue, calculatedValue, AnimationDuration)
                    .setEase(animationEaseOut)
                    .setOnUpdate(val =>
                    {
                        _owner.ChangeCharacterStat(statId, val);
                    });
            }

            foreach (WeaponStatChange statChange in boost.WeaponStatChanges)
            {
                statId = (int)statChange.StatEnum;
                currentValue = _owner.GetWeaponStat(statId);
                calculatedValue = currentValue - statChange.Value;
                LeanTween.value(currentValue, calculatedValue, AnimationDuration)
                    .setEase(animationEaseOut)
                    .setOnUpdate(val =>
                    {
                        _owner.ChangeWeaponStat(statId, val);
                    });
            }
        }
    }

    public void RemoveBoostImmediate()
    {
        foreach (Upgrade boost in Boosts)
        {
            int statId;
            float currentValue;
            float calculatedValue;
            foreach (CharacterStatChange statChange in boost.CharacterStatChanges)
            {
                statId = (int)statChange.StatEnum;
                currentValue = _owner.GetCharacterStat(statId);
                calculatedValue = currentValue - statChange.Value;
                _owner.ChangeCharacterStat(statId, calculatedValue);
            }

            foreach (WeaponStatChange statChange in boost.WeaponStatChanges)
            {
                statId = (int)statChange.StatEnum;
                currentValue = _owner.GetWeaponStat(statId);
                calculatedValue = currentValue - statChange.Value;
                _owner.ChangeWeaponStat(statId, calculatedValue);
            }
        }
        Enabled = false;
    }

    private void StartScreenFX(Blit screenFX)
    {
        Debug.Log("StartScreenFX");

        screenFX.SetActive(true);
        Material blitMat = screenFX.settings.blitMaterial;
        Debug.Log(blitMat.GetFloat("_Intensity"));
        InAnimation = true;

        LeanTween.value(0, 1, AnimationDuration)
            .setEase(animationEaseIn)
            .setOnUpdate(l =>
            {
                blitMat.SetFloat("_Intensity", l);
            })
            .setOnComplete(l => 
            {
                InAnimation = false;
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
            .setEase(animationEaseOut)
            .setOnUpdate(l =>
            {
                blitMat.SetFloat("_Intensity", l);
            })
            .setOnComplete(l =>
            {
                InAnimation = false;
                screenFX.SetActive(false);
                Enabled = false;
            });
    }
}
