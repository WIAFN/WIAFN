using UnityEditor.PackageManager;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public LayerMask gunLookLayerMask;
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
            playerWeapon.TryShoot(hit.point);
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
        Vector3 gunLocalPos = playerWeapon.transform.parent.InverseTransformPoint(hit.point);
        if (hit.transform == null || playerWeapon.transform.rotation.y < -15f)
        {
            gunLocalPos = Quaternion.Euler(new Vector3(0f, -30f, -90f)) * Vector3.forward;
        }
        playerWeapon.transform.localRotation = Quaternion.LookRotation(gunLocalPos - playerWeapon.transform.localPosition);
    }

    public delegate void InteractHandler();
}
