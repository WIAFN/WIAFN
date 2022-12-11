using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterBaseStats))]
public class Character : MonoBehaviour
{
    private CharacterMovement _characterMove;
    private CharacterBaseStats _baseStats;

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
            if (OnDied != null)
            {
                OnDied();
            }

            Destroy(gameObject);
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

    public CharacterMovement CharacterMovement => _characterMove;

    public delegate void DamageTakeHandler(float damageTaken);
    public delegate void VoidHandler();
}
