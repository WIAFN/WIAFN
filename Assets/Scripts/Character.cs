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

    public int keyItems { get; private set; }

    public string state { get; set; }


    public event DamageTakeHandler OnDamageTaken;
    public event VoidHandler OnDied;


    #region STAT UPPER AND LOWER BOUNDS
    //WEAPON
    private readonly float WEAPON_MIN_DMG = 0.5f;
    private readonly float WEAPON_MAX_DMG = 300f;

    private readonly float WEAPON_MIN_FR = 0.1f;
    private readonly float WEAPON_MAX_FR = 100f;

    //CHARACTER
    private readonly float CHAR_MIN_HP = 50f;
    private readonly float CHAR_MAX_HP = 3000f;

    private readonly float CHAR_MIN_STM = 100f;
    private readonly float CHAR_MAX_STM = 2000f;

    private readonly float CHAR_MIN_HPREG = 0.1f;
    private readonly float CHAR_MAX_HPREG = 100f;

    private readonly float CHAR_MIN_STMREG = 1f;
    private readonly float CHAR_MAX_STMREG = 50f;

    private readonly float CHAR_MIN_JUMP_HEIGHT = 2f;
    private readonly float CHAR_MAX_JUMP_HEIGHT = 10f;

    private readonly int CHAR_MIN_MULTIJMP = 1;
    private readonly int CHAR_MAX_MULTIJMP = 5;

    private readonly float CHAR_MIN_SPEED = 6f;
    private readonly float CHAR_MAX_SPEED = 30f;

    private readonly float CHAR_MIN_SPRINT = 30f;
    private readonly float CHAR_MAX_SPRINT = 120f;

    private readonly float CHAR_MIN_DASH_SPEED = 120f;
    private readonly float CHAR_MAX_DASH_SPEED = 600f;

    private readonly int CHAR_MIN_MULTIDASH = 0;
    private readonly int CHAR_MAX_MULTIDASH = 5;

    private readonly float CHAR_MIN_DASH_DUR = 0.05f;
    private readonly float CHAR_MAX_DASH_DUR = 1f;

    private readonly float CHAR_MIN_DASH_CD = 2f;
    private readonly float CHAR_MAX_DASH_CD = 10f;

    private readonly float CHAR_MIN_DASH_COST = 10f;
    private readonly float CHAR_MAX_DASH_COST = 200f;

    private readonly float CHAR_MIN_SPDCOEF = 0.3f;
    private readonly float CHAR_MAX_SPDCOEF = 2f;
    #endregion

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
            case -1:
                return keyItems;
            case 0:
                return BaseStats.maxHealth;
            case 1:
                return BaseStats.maxStamina;
            case 2:
                return BaseStats.healthRegen;
            case 3:
                return BaseStats.staminaRegen;
            case 4:
                return BaseStats.jumpHeight;
            case 5:
                return BaseStats.multiJumps;
            case 6:
                return BaseStats.defaultSpeed;
            case 7:
                return BaseStats.defaultSprintSpeed;
            case 8:
                return BaseStats.defaultDashSpeed;
            case 9:
                return BaseStats.multiDashes;
            case 10:
                return BaseStats.dashDuration;
            case 11:
                return BaseStats.dashCooldown;
            case 12:
                return BaseStats.dashCost;
            case 13:
                return BaseStats.speedCoefficient;
            default:
                return BaseStats.maxHealth;
        }
    }
    public void ChangeCharacterStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case -1:
                keyItems += (int)value;
                break;
            case 0:
                BaseStats.maxHealth = Mathf.Clamp(BaseStats.maxHealth + value, CHAR_MIN_HP, CHAR_MAX_HP);
                break;
            case 1:
                BaseStats.maxStamina = Mathf.Clamp(BaseStats.maxStamina + value, CHAR_MIN_STM, CHAR_MAX_STM);
                break;
            case 2:
                BaseStats.healthRegen = Mathf.Clamp(BaseStats.healthRegen + value, CHAR_MIN_HPREG, CHAR_MAX_HPREG);
                break;
            case 3:
                BaseStats.staminaRegen = Mathf.Clamp(BaseStats.staminaRegen + value, CHAR_MIN_STMREG, CHAR_MAX_STMREG);
                break;
            case 4:
                BaseStats.jumpHeight = Mathf.Clamp(BaseStats.jumpHeight + value, CHAR_MIN_JUMP_HEIGHT, CHAR_MAX_JUMP_HEIGHT);
                break;
            case 5:
                BaseStats.multiJumps = Mathf.Clamp(BaseStats.multiJumps + Mathf.CeilToInt(value), CHAR_MIN_MULTIJMP, CHAR_MAX_MULTIJMP);
                break;
            case 6:
                BaseStats.defaultSpeed = Mathf.Clamp(BaseStats.defaultSpeed + value, CHAR_MIN_SPEED, CHAR_MAX_SPEED);
                break;
            case 7:
                BaseStats.defaultSprintSpeed = Mathf.Clamp(BaseStats.defaultSprintSpeed + value, CHAR_MIN_SPRINT, CHAR_MAX_SPRINT);
                break;
            case 8:
                BaseStats.defaultDashSpeed = Mathf.Clamp(BaseStats.defaultDashSpeed + value, CHAR_MIN_DASH_SPEED, CHAR_MAX_DASH_SPEED);
                break;
            case 9:
                BaseStats.multiDashes = Mathf.Clamp(BaseStats.multiDashes + Mathf.CeilToInt(value), CHAR_MIN_MULTIDASH, CHAR_MAX_MULTIDASH);
                break;
            case 10:
                BaseStats.dashDuration = Mathf.Clamp(BaseStats.dashDuration + value, CHAR_MIN_DASH_DUR, CHAR_MAX_DASH_DUR);
                break;
            case 11:
                BaseStats.dashCooldown = Mathf.Clamp(BaseStats.dashCooldown + value, CHAR_MIN_DASH_CD, CHAR_MAX_DASH_CD);
                break;
            case 12:
                BaseStats.dashCost = Mathf.Clamp(BaseStats.dashCost + value, CHAR_MIN_DASH_COST, CHAR_MAX_DASH_COST);
                break;
            case 13:
                BaseStats.speedCoefficient = Mathf.Clamp(BaseStats.speedCoefficient + value, CHAR_MIN_SPDCOEF, CHAR_MAX_SPDCOEF);
                break;
        }
    }

    public void SetCharacterStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case -1:
                keyItems += (int)value;
                break;
            case 0:
                BaseStats.maxHealth = Mathf.Clamp(value, CHAR_MIN_HP, CHAR_MAX_HP);
                break;
            case 1:
                BaseStats.maxStamina = Mathf.Clamp(value, CHAR_MIN_STM, CHAR_MAX_STM);
                break;
            case 2:
                BaseStats.healthRegen = Mathf.Clamp(value, CHAR_MIN_HPREG, CHAR_MAX_HPREG);
                break;
            case 3:
                BaseStats.staminaRegen = Mathf.Clamp(value, CHAR_MIN_STMREG, CHAR_MAX_STMREG);
                break;
            case 4:
                BaseStats.jumpHeight = Mathf.Clamp(value, CHAR_MIN_JUMP_HEIGHT, CHAR_MAX_JUMP_HEIGHT);
                break;
            case 5:
                BaseStats.multiJumps = Mathf.Clamp((int)value, CHAR_MIN_MULTIJMP, CHAR_MAX_MULTIJMP);
                break;
            case 6:
                BaseStats.defaultSpeed = Mathf.Clamp(value, CHAR_MIN_SPEED, CHAR_MAX_SPEED);
                break;
            case 7:
                BaseStats.defaultSprintSpeed = Mathf.Clamp(value, CHAR_MIN_SPRINT, CHAR_MAX_SPRINT);
                break;
            case 8:
                BaseStats.defaultDashSpeed = Mathf.Clamp(value, CHAR_MIN_DASH_SPEED, CHAR_MAX_DASH_SPEED);
                break;
            case 9:
                BaseStats.multiDashes = Mathf.Clamp((int)value, CHAR_MIN_MULTIDASH, CHAR_MAX_MULTIDASH);
                break;
            case 10:
                BaseStats.dashDuration = Mathf.Clamp(value, CHAR_MIN_DASH_DUR, CHAR_MAX_DASH_DUR);
                break;
            case 11:
                BaseStats.dashCooldown = Mathf.Clamp(value, CHAR_MIN_DASH_CD, CHAR_MAX_DASH_CD);
                break;
            case 12:
                BaseStats.dashCost = Mathf.Clamp(value, CHAR_MIN_DASH_COST, CHAR_MAX_DASH_COST);
                break;
            case 13:
                BaseStats.speedCoefficient = Mathf.Clamp(value, CHAR_MIN_SPDCOEF, CHAR_MAX_SPDCOEF);
                break;
        }
    }

    public void ChangeWeaponStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case 0:
                Weapon.Damage = Mathf.Clamp(Weapon.Damage + value, WEAPON_MIN_DMG, WEAPON_MAX_DMG);
                break;
            case 1:
                Weapon.FireRate = Mathf.Clamp(Weapon.FireRate + value, WEAPON_MIN_FR, WEAPON_MAX_FR);
                break;
        }
    }

    public void SetWeaponStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case 0:
                Weapon.Damage = Mathf.Clamp(value, WEAPON_MIN_DMG, WEAPON_MAX_DMG);
                break;
            case 1:
                Weapon.FireRate = Mathf.Clamp(value, WEAPON_MIN_FR, WEAPON_MAX_FR);
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
        if (effect.Boosts.Count == 0) return;
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

    public void ResetKeyItems()
    {
        keyItems = 0;
    }

    public CharacterMovement CharacterMovement => _characterMove;

    public delegate void DamageTakeHandler(float damageTaken);
    public delegate void VoidHandler();
}
