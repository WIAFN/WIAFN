using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarChange : MonoBehaviour
{
    private Character _character;
    private Image HealthBar;
    private float CurrentHealth;

    public void Start()
    {
        HealthBar = GetComponent<Image>();
        _character = GameManager.instance.mainPlayer;
    }

    private void Update()
    {
        CurrentHealth = _character.health;
        HealthBar.fillAmount = CurrentHealth / _character.BaseStats.maxHealth;
    }
}
