using UnityEngine;

[RequireComponent(typeof(Transform))]
public abstract class Weapon : MonoBehaviour
{
    [HideInInspector]
    public Character Character;

    public float FireRate;
    public float Damage;
    public Transform GunTip;
    public ParticleSystem MuzzleFlash;
    public float Delay { get; protected set; }

    public GameObject projectilePrefab;

    private float _firePeriod { get { return (1f / FireRate); } }

    public event ShotHandler OnShot; 

    private void Start()
    {
        Delay = 0;

        if (Character == null)
        {
            Character = GetComponentInParent<Character>();
        }
    }

    private void Update()
    {
        if(Delay < _firePeriod)
        {
            Delay += Time.deltaTime;        
        }
        Delay = Mathf.Clamp(Delay, 0, _firePeriod);
    }

    public bool TryShoot(Vector3 shootAt)
    {
        if (Delay < _firePeriod)
        {
            return false;
        }

        float shootingError = 0f;
        if (Character != null)
        {
            shootingError = Character.GetFiringErrorRate();
        }
        shootAt = AddShootingError(shootAt, shootingError);

        Vector3 aimVector = (shootAt - GunTip.position).normalized;
        GameObject projectileGameObject = Instantiate(projectilePrefab, GunTip.position, Quaternion.LookRotation(aimVector, Vector3.up));
        Projectile projectile = projectileGameObject.GetComponent<Projectile>();

        projectile.SetWeapon(this);

        //Gun Flare
        MuzzleFlash.Play();
        Delay = 0;

        OnShot?.Invoke(projectile);

        return true;
    }

    private Vector3 AddShootingError(Vector3 shootAt, float errorRateToDistance)
    {
        float distance = Vector3.Distance(shootAt, transform.position);
        return shootAt + Random.insideUnitSphere * distance * errorRateToDistance;
    }

    public delegate void ShotHandler(Projectile projectile);
}
