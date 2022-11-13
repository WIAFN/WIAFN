using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GunCamera : MonoBehaviour
{
    public LayerMask gunLookLayerMask;
    public GameObject pfBullet;
    public Transform gunTip;
    public PlayerWeapon playerWeapon;
    private bool _isShooting = false;


    // Update is called once per frame
    public Camera fpsCam;
    void Update()
    {
        Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, 100.0f, gunLookLayerMask);
        RotateGun(hit);

        if (_isShooting && hit.transform != null)
        {
            playerWeapon.Shoot(hit.point, pfBullet);
        }

        if (Input.GetMouseButtonDown(0))
        {
            _isShooting = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isShooting = false;
        }

    }

    private void RotateGun(RaycastHit hit)
    {
        Vector3 gunLocalPos = transform.parent.InverseTransformPoint(hit.point);
        if (hit.transform == null || transform.rotation.y < -15f)
        {
            gunLocalPos = Quaternion.Euler(new Vector3(30f, -50f, 0f)) * Vector3.forward;
        }
        transform.localRotation = Quaternion.LookRotation(gunLocalPos - transform.localPosition);
    }
}
