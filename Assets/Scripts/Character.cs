using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(CharacterBaseStats))]
public class Character : MonoBehaviour
{
    private CharacterMovement _characterMove;
    private CharacterBaseStats _baseStats;
    private Weapon _weapon;
    private Effect _effect;

    //Getter
    public Weapon Weapon => _weapon;
    public Effect Effect => _effect;

    // Runtime Stats
    [HideInInspector]
    public float health { get; private set; }

    [HideInInspector]
    public float stamina { get; private set; }

    public event DamageTakeHandler OnDamageTaken;
    public event VoidHandler OnDied;

    private void Awake()
    {
        _characterMove = GetComponent<CharacterMovement>();
        _baseStats = GetComponent<CharacterBaseStats>();
        _weapon = GetComponentInChildren<Weapon>();
    }

    public void Start()
    {

        health = _baseStats.maxHealth;
        stamina = _baseStats.maxStamina;
    }

    public void Update()
    {
        if (health <= 0)
        {
            Die();
        }

        if (_characterMove != null)
        {
            if (!_characterMove.IsSprinting && !_characterMove.IsDashing)
            {
                if (stamina < _baseStats.maxStamina)
                {
                    RegenStamina();
                }
            }
        }

        if (health < _baseStats.maxHealth)
        {
            RegenHealth();
        }
    }

    private void Die()
    {
        if (OnDied != null)
        {
            OnDied();
        }

        GameManager.instance.CharacterDied(this);

        Destroy(gameObject);
    }

    public void RegenStamina()
    {
        stamina += _baseStats.staminaRegen * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, _baseStats.maxStamina);
    }

    public void RemoveStamina(float stamina)
    {
        this.stamina -= stamina;
    }

    public void RegenHealth()
    {
        health += _baseStats.healthRegen * Time.deltaTime;
        health = Mathf.Clamp(health, 0, _baseStats.maxHealth);
    }

    public void RemoveHealth(float health)
    {
        this.health -= health;

        if (OnDamageTaken != null)
        {
            OnDamageTaken(health);
        }
    }

    public float GetFiringErrorRate()
    {
        Vector2 shootingErrorMinMax = BaseStats.shootingErrorRateMinMax;
        float shootingError = RangeUtilities.map(_characterMove.Speed, 0f, BaseStats.MaxRunSpeed, shootingErrorMinMax.x, shootingErrorMinMax.y);
        return shootingError;
    }

    public CharacterBaseStats BaseStats
    {
        get
        {
            return _baseStats;
        }
    }

    public float GetCharacterStat(int attributeEnum)
    {
        switch (attributeEnum)
        {
            case 0:
                return BaseStats.maxHealth;
            case 1:
                return BaseStats.maxStamina;
            case 2:
                return BaseStats.staminaRegen;
            case 3:
                return BaseStats.healthRegen;
            case 4:
                return BaseStats.speedCoefficient;
            default:
                return BaseStats.maxHealth;
        }
    }
    public void ChangeCharacterStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case 0:
                BaseStats.maxHealth += value;
                break;
            case 1:
                BaseStats.maxStamina += value;
                break;
            case 2:
                BaseStats.healthRegen += value;
                break;
            case 3:
                BaseStats.staminaRegen += value;
                break;
            case 4:
                BaseStats.speedCoefficient += value;
                break;
        }
    }

    public void ChangeWeaponStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case 0:
                Weapon.Damage = value;
                break;
            case 1:
                Weapon.FireRate = value;
                break;
        }
    }

    public float GetWeaponStat(int attributeEnum)
    {
        switch (attributeEnum)
        {
            case 0:
                return Weapon.Damage;
            case 1:
                return Weapon.FireRate;
        }
        return 0;
    }

    public void ChangeEffect(Effect effect)
    {
        if (HasEffect())
        {
            _effect.OnEffectDrop();
        }
        _effect = effect;
    }

    public bool HasEffect()
    {
        return _effect != null;
    }

    public CharacterMovement CharacterMovement => _characterMove;

    public delegate void DamageTakeHandler(float damageTaken);
    public delegate void VoidHandler();
}
