using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class GunCamera : MonoBehaviour
{
    public LayerMask gunLookLayerMask;

    // Update is called once per frame
    public Camera fpsCam;
    void Update()
    {
        Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, 100.0f, gunLookLayerMask);

        Vector3 targetLocalPos = transform.parent.InverseTransformPoint(hit.point);

        if (hit.transform == null)
        {
            targetLocalPos = Quaternion.Euler(new Vector3(30f, -50f, 0f)) * Vector3.forward;
        }
        transform.localRotation = Quaternion.LookRotation(targetLocalPos - transform.localPosition);
    }
}
