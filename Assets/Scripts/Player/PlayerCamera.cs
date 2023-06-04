using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //public LayerMask gunLookLayerMask;
    private Character _player;
    private bool _isShooting = false;

    public static event InteractHandler OnInteract;

    private void Start()
    {
        _player = GetComponent<Character>();
    }

    // Update is called once per frame
    public Camera fpsCam;
    void Update()
    {
        //Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, 100.0f, gunLookLayerMask);
        Ray ray = new Ray(fpsCam.transform.position, fpsCam.transform.forward);
        //RotateGun(hit);
        // && hit.transform != null
        if (_isShooting)
        {
            _player.Weapon?.TryShoot(ray.GetPoint(1000f));
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
        if (Input.GetKeyDown(KeyCode.R) && _player.HasEffect())
        {
            if(!_player.Effect.InAnimation)
            {
                if (!_player.Effect.Enabled)
                {
                    _player.Effect.OnEffectStart();
                }
                else _player.Effect.OnEffectEnd();
            }
        }
    }

    private void RotateGun(RaycastHit hit)
    {
        Vector3 gunLocalPos = _player.Weapon.transform.parent.InverseTransformPoint(hit.point);
        if (hit.transform == null || _player.Weapon.transform.rotation.y < -15f)
        {
            gunLocalPos = Quaternion.Euler(new Vector3(0f, -30f, -90f)) * Vector3.forward;
        }
        _player.Weapon.transform.localRotation = Quaternion.LookRotation(gunLocalPos - _player.Weapon.transform.localPosition);
    }

    public delegate void InteractHandler();
}
