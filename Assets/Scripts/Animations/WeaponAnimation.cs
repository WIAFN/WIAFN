using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIAFN.Constants;

public class WeaponAnimation : MonoBehaviour
{

    private Weapon _weapon;
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();

        _weapon = GetComponent<Weapon>();
        _weapon.OnShot += OnShot;
    }

    void OnShot(Projectile projectile)
    {
        _animator.SetTrigger(WIAFNAnimatorParams.Attacked);
    }

    private void OnDestroy()
    {
        _weapon.OnShot -= OnShot;
    }
}
