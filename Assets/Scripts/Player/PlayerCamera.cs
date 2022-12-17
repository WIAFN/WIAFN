using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public LayerMask gunLookLayerMask;
    public GameObject pfBullet;
    public PlayerWeapon playerWeapon;
    private bool _isShooting = false;

    public static event InteractHandler OnInteract;


    // Update is called once per frame
    public Camera fpsCam;
    void Update()
    {
        Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, 100.0f, gunLookLayerMask);
        //RotateGun(hit);

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

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(OnInteract != null)
            {
                OnInteract();
            }
        }

    }

    private void RotateGun(RaycastHit hit)
    {
        Vector3 gunLocalPos = transform.parent.InverseTransformPoint(hit.point);
        if (hit.transform == null || transform.rotation.y < -15f)
        {
            gunLocalPos = Quaternion.Euler(new Vector3(0f, -30f, -90f)) * Vector3.forward;
        }
        transform.localRotation = Quaternion.LookRotation(gunLocalPos - transform.localPosition);
    }

    public delegate void InteractHandler();
}
