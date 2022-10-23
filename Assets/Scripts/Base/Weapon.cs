using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Transform))]
public abstract class Weapon : MonoBehaviour
{
    public float fireRate;
    public CharacterMove characterMove;
    public Transform gunTip;
    public float Delay { get; protected set; }

    private float _firePeriod { get { return (1f / fireRate); } }

    private void Start()
    {
        gunTip = transform.GetChild(0);
        Delay = 0;
    }

    private void Update()
    {
        if(Delay < _firePeriod)
        {
            Delay += Time.deltaTime;        
        }
        Delay = Mathf.Clamp(Delay, 0, _firePeriod);
    }
    public void Shoot(Vector3 shootAt, GameObject projectilePrefab)
    {
        if(Delay < _firePeriod)
        {
            return;
        }
        Vector3 aimVector = (shootAt - gunTip.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, gunTip.position, Quaternion.LookRotation(aimVector, Vector3.up));
        Projectile a = projectile.GetComponent<Projectile>();
        a.SetInitialVelocity(characterMove.Velocity);
        Delay = 0;
    }
}
