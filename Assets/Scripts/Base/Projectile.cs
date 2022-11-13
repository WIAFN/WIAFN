using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float speed;                     //Unit per second?
    public float timeToLive;                //Time to live (second)
    public float damage;
    public int penetrationValue;
    public GameObject impactParticleSystem; //Impact
    public GameObject impactHole;

    protected TrailRenderer TrailRenderer;  //Bullet trail?

    private Vector3 _oldPos;
    private Vector3 _parentVelocity;        //Parent velocity

    private void Awake()
    {
        TrailRenderer = transform.GetChild(0).GetComponent<TrailRenderer>();
    }

    protected virtual void Start()
    {
        _oldPos = transform.position;
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
            if (penetrationValue < 0) break;

            HitObject(hit);
        }

        //Check if bullet will hit anything in front of it
        if (penetrationValue >= 0 && speed < 400f)
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
        //Remove one pen value, might change it depending on character armor values, etc.
        penetrationValue--;

        //Get normal of the impact surface
        Vector3 reflect = Vector3.Reflect(transform.forward, hit.normal);

        //On hit particle System
        GameObject ps = Instantiate(impactParticleSystem, hit.point, Quaternion.LookRotation(reflect, Vector3.up));
        Destroy(ps, 0.3f);

        //Impact point
        GameObject ip = Instantiate(impactHole, hit.point, Quaternion.LookRotation(-transform.forward, transform.up));
        Debug.DrawRay(ip.transform.position, ip.transform.forward, Color.red);
        Destroy(ip, 3f);

        GameObject hitObject = hit.transform.gameObject;
        Character hitCharacter = hitObject.GetComponent<Character>();
        if (hitCharacter != null)
        {
            hitCharacter.RemoveHealth(damage);
        }
        //Destroy self if cannot penetrate anymore
        if (penetrationValue < 0)
        {
            Destroy(gameObject);
        }
    }

    private void MoveProjectile()
    {
        _oldPos = transform.position;
        //Keep the initial momentum
        transform.position += _parentVelocity * Time.deltaTime;
        //Forward thrust
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void ProjectileTTL()
    {
        if (timeToLive < 0)
        {
            Destroy(gameObject);
        }
        timeToLive -= Time.deltaTime;
    }

    public void SetInitialVelocity(Vector3 parentVelocity)
    {
        parentVelocity.y = 0;
        _parentVelocity = parentVelocity;
    }
}