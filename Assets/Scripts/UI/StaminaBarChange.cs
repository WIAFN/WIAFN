using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarChange : MonoBehaviour
{
    private Character _character;
    private Image StaminaBar;
    private float CurrentStamina;

    public void Start()
    {
        StaminaBar = GetComponent<Image>();
        _character = GameManager.instance.mainPlayer;
    }

    private void Update()
    {
        if (_character.isActiveAndEnabled)
        {
            CurrentStamina = _character.stamina;
            StaminaBar.fillAmount = CurrentStamina / _character.BaseStats.maxStamina;
        }
    }
}
