using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    public float meleeRange;
    public float meleeAngle;
    private AudioManager audioManager;


    public override bool TryShoot(Vector3 shootAt)
    {
        // Check if the target is within melee range and angle
        Vector3 toTarget = shootAt - transform.position;
        float distanceToTarget = toTarget.magnitude;
        float angleToTarget = Vector3.Angle(transform.forward, toTarget);
        audioManager = FindObjectOfType<AudioManager>();

        if (distanceToTarget <= meleeRange && angleToTarget <= meleeAngle)
        {
            Debug.Log("melee attack");
            Delay = _firePeriod;
            AudioManager.instance?.PlayMeleeSound(transform);

            return true;
        }
        else
        {
            // If target is out of range or angle call base TryShoot method
            if (base.TryShoot(shootAt))
            {
                Delay = _firePeriod;
                AudioManager.instance?.PlayMeleeSound(transform);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
