using UnityEngine;


[RequireComponent(typeof(Transform))]
public abstract class Weapon : MonoBehaviour
{
    [HideInInspector]
    public Character character;
    private CharacterMovement _characterMove;

    public float fireRate;
    public Transform gunTip;
    public ParticleSystem muzzleFlash;
    public float Delay { get; protected set; }

    public GameObject projectilePrefab;

    private float _firePeriod { get { return (1f / fireRate); } }

    public event ShotHandler OnShot; 

    private void Start()
    {
        Delay = 0;

        if (character == null)
        {
            character = GetComponentInParent<Character>();
        }

        _characterMove = character.GetComponent<CharacterMovement>();
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
        if (character != null)
        {
            shootingError = character.GetFiringErrorRate();
        }
        shootAt = AddShootingError(shootAt, shootingError);

        Vector3 aimVector = (shootAt - gunTip.position).normalized;
        GameObject projectileGameObject = Instantiate(projectilePrefab, gunTip.position, Quaternion.LookRotation(aimVector, Vector3.up));
        Projectile projectile = projectileGameObject.GetComponent<Projectile>();

        if (_characterMove != null)
        {
            projectile.SetInitialVelocity(_characterMove.Velocity);
        }

        //Gun Flare
        muzzleFlash.Play();
        Delay = 0;
        
        if (OnShot != null)
        {
            OnShot(projectile);
        }

        return true;
    }

    private Vector3 AddShootingError(Vector3 shootAt, float errorRateToDistance)
    {
        float distance = Vector3.Distance(shootAt, transform.position);
        return shootAt + Random.insideUnitSphere * distance * errorRateToDistance;
    }

    public delegate void ShotHandler(Projectile projectile);
}
