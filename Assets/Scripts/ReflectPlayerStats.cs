using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReflectPlayerStats : MonoBehaviour
{
    public PlayerAttributes attributes;
    public Slider HPBar;
    public Slider STMBar;

    // Update is called once per frame
    void Update()
    {
        HPBar.value = Mathf.InverseLerp(0, attributes.maxHealth, attributes.health);
        STMBar.value = Mathf.InverseLerp(0, attributes.maxStamina, attributes.stamina);
    }
}
