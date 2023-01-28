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

    //Getter
    public Weapon Weapon => _weapon;

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
                BaseStats.staminaRegen += value;
                break;
            case 3:
                BaseStats.healthRegen += value;
                break;
            case 4:
                BaseStats.speedCoefficient += value;
                break;
            case 14:
                Weapon.damage += value;
                break;
            case 15:
                Weapon.fireRate += value;
                break;
        }
    }

    public void ChangeWeaponStat(int attributeEnum, float value)
    {
        switch (attributeEnum)
        {
            case 0:
                Weapon.damage += value;
                break;
            case 1:
                Weapon.fireRate += value;
                break;
        }
    }

    public CharacterMovement CharacterMovement => _characterMove;

    public delegate void DamageTakeHandler(float damageTaken);
    public delegate void VoidHandler();
}
