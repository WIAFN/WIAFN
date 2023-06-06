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

    protected float FirePeriod { get { return (1f / FireRate); } }

    public event ShotHandler OnShot;
    private AudioManager audioManager;

    private void Start()
    {
        Delay = 0;

        if (Character == null)
        {
            Character = GetComponentInParent<Character>();
        }

        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        if(Delay < FirePeriod)
        {
            Delay += Time.deltaTime;        
        }
        Delay = Mathf.Clamp(Delay, 0, FirePeriod);
    }

    public virtual bool TryShoot(Vector3 shootAt)
    {
        if (Delay < FirePeriod)
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

        // View effects
        //Gun Flare
        MuzzleFlash.Play();
        Delay = 0;

        AudioManager.instance.PlayCharGunshot(transform);

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
