using UnityEngine;

[RequireComponent(typeof(Transform))]
public abstract class Weapon : MonoBehaviour
{
    public float fireRate;
    public float damage;
    public CharacterMovement characterMove;
    public Transform gunTip;
    public ParticleSystem muzzleFlash;
    public float Delay { get; protected set; }

    private float _firePeriod { get { return (1f / fireRate); } }

    private void Start()
    {
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
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.SetInitialVelocity(characterMove.VerticalVelocity);
        proj.SetDamage(damage);
        //Gun Flare
        muzzleFlash.Play();
        Delay = 0;
    }
}
