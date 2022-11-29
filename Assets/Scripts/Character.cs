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
    public float health;

    [HideInInspector]
    public float stamina;


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

    public void RemoveHealth(float health)
    {
        this.health -= health;
    }

    public CharacterBaseStats BaseStats
    {
        get
        {
            return _baseStats;
        }
    }
}
