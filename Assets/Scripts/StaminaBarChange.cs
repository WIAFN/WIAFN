using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarChange : MonoBehaviour
{
    private Character _character;
    private Image StaminaBar;
    public float CurrentStamina;
    private float MaxStamina = 100f;

    public void Start()
    {
        StaminaBar = GetComponent<Image>();
    }

    private void Update()
    {
        CurrentStamina = _character.stamina;
        StaminaBar.fillAmount = CurrentStamina / MaxStamina;
    }
}
