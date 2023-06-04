using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReflectPlayerStats : MonoBehaviour
{
    private Character _character;
    public Weapon playerWeapon;
    public Slider HPBar;
    public Slider STMBar;
    public Slider FireRate;

    public void Start()
    {
        _character = GameManager.instance.mainPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        HPBar.value = Mathf.InverseLerp(0, _character.BaseStats.maxHealth, _character.health);
        STMBar.value = Mathf.InverseLerp(0, _character.BaseStats.maxStamina, _character.stamina);
        FireRate.value = Mathf.InverseLerp(0, playerWeapon.FireRate, playerWeapon.Delay);

    }
}
