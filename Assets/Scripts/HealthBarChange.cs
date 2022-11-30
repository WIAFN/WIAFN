using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarChange : MonoBehaviour
{
    private Character _character;
    private Image HealthBar;
    public float CurrentHealth;
    private float MaxHealth = 100f;

    public void Start()
    {
        HealthBar = GetComponent<Image>();
    }

    private void Update()
    {
        CurrentHealth = _character.health;
        HealthBar.fillAmount = CurrentHealth / MaxHealth;
    }
}
