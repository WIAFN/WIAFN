using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float Speed;                     //Unit per second?
    public float TimeToLive;                //Time to live (second)
    public int PenetrationValue;
    public GameObject ImpactParticleSystem; //Impact
    public GameObject ImpactHole;

    [HideInInspector]
    public Weapon Weapon;

    protected TrailRenderer TrailRenderer;  //Bullet trail?

    private Vector3 _oldPos;
    private Vector3 _parentVelocity;        //Parent velocity
    private MeshRenderer _meshRenderer;     //Get mesh renderer from child
    private float _damage;                   //Moved from projectile, might do terraria logic for bullet + weapon damage combo

    public float Damage => _damage;

    private void Awake()
    {
        TrailRenderer = GetComponent<TrailRenderer>();
        //FIRST AND ONLY CHILD IS ALWAYS THE MESH
        _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    protected virtual void Start()
    {
        _oldPos = transform.position;
        _meshRenderer.enabled = false;
        StartCoroutine(EnableMesh());
    }

    private IEnumerator EnableMesh() 
    {
        yield return null;
        _meshRenderer.enabled = true;
    }

    // Update is called once per frame
    private void Update()
    {
        ProjectileHitCheck();
        ProjectileTTL();
        MoveProjectile();
    }

    private void ProjectileHitCheck()
    {
        //Check if bullet have passed through something
        var hitList = Physics.RaycastAll(_oldPos, (transform.position - _oldPos).normalized, Vector3.Distance(transform.position,_oldPos));
        foreach(var hit in hitList)
        {
            if (PenetrationValue < 0) break;

            HitObject(hit);
        }

        //Check if bullet will hit anything in front of it
        if (PenetrationValue >= 0 && Speed < 400f)
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit forwardHit, transform.localScale.x / 3);
            if (forwardHit.transform != null)
            {
                HitObject(forwardHit);
            }
        }
    }

    private void HitObject(RaycastHit hit)
    {
        //Get hit information
        GameObject hitObject = hit.transform.gameObject;
        Character hitCharacter = hitObject.GetComponent<Character>();

        if(hitCharacter == Weapon.Character)
        {
            Debug.Log("Hitting self");
            return;
        }

        //Remove one pen value, might change it depending on character armor values, etc.
        PenetrationValue--;

        //Get normal of the impact surface
        Vector3 reflect = Vector3.Reflect(transform.forward, hit.normal);

        //On hit particle System
        GameObject ps = Instantiate(
            ImpactParticleSystem,
            hit.point + (hit.normal * .01f),
            Quaternion.LookRotation(reflect, Vector3.up));
        Destroy(ps, 0.3f);

        //Impact point
        if(hitCharacter != GameManager.instance.mainPlayer)
        {
            GameObject ip = Instantiate(
                ImpactHole,
                hit.point + (hit.normal * .01f),
                Quaternion.LookRotation(hit.normal)
                );
            ip.transform.parent = hit.transform;

            Destroy(ip, 10f);
        }


        if (hitCharacter != null)
        {
            hitCharacter.Damage(this);
        }

        //Destroy self if cannot penetrate anymore
        if (PenetrationValue < 0)
        {
            Destroy(gameObject);
        }

        AudioManager.instance?.PlayMetalicSound(transform);
    }

    private void MoveProjectile()
    {
        _oldPos = transform.position;
        //Keep the initial momentum
        transform.position += _parentVelocity * Time.deltaTime;
        //Forward thrust
        transform.position += Speed * Time.deltaTime * transform.forward;
    }

    private void ProjectileTTL()
    {
        if (TimeToLive < 0)
        {
            Destroy(gameObject);
        }
        TimeToLive -= Time.deltaTime;
    }

    public void SetInitialVelocity(Vector3 parentVelocity)
    {
        parentVelocity.y = 0;
        _parentVelocity = parentVelocity;
    }

    public void SetWeapon(Weapon weapon)
    {
        this.Weapon = weapon;
        this._damage = weapon.Damage;
    }
}
