using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public Camera fpsCam;
    public Camera tpsCam;
    // Start is called before the first frame update
    void Start()
    {
        fpsCam.enabled = true;
        tpsCam.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            fpsCam.enabled = !fpsCam.enabled;
            tpsCam.enabled = !tpsCam.enabled;
        }
    }
}
