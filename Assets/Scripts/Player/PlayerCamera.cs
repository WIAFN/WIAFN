using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public LayerMask gunLookLayerMask;
    public Character Player;
    private bool _isShooting = false;
    private bool _inEffect = false;

    public static event InteractHandler OnInteract;


    // Update is called once per frame
    public Camera fpsCam;
    void Update()
    {
        Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, 100.0f, gunLookLayerMask);
        //RotateGun(hit);

        if (_isShooting && hit.transform != null)
        {
            Player.Weapon.TryShoot(hit.point);
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

        //three ifs, why?!
        if (Input.GetKeyDown(KeyCode.R))
        {
            if(!Player.Effect.InAnimation)
            {
                if (!_inEffect)
                {
                    Player.Effect.OnEffectStart();
                }
                else Player.Effect.OnEffectEnd();
                _inEffect = !_inEffect;
            }
        }
    }

    private void RotateGun(RaycastHit hit)
    {
        Vector3 gunLocalPos = Player.Weapon.transform.parent.InverseTransformPoint(hit.point);
        if (hit.transform == null || Player.Weapon.transform.rotation.y < -15f)
        {
            gunLocalPos = Quaternion.Euler(new Vector3(0f, -30f, -90f)) * Vector3.forward;
        }
        Player.Weapon.transform.localRotation = Quaternion.LookRotation(gunLocalPos - Player.Weapon.transform.localPosition);
    }

    public delegate void InteractHandler();
}
