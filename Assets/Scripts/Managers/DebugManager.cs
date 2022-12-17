using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;
    public GameObject pfUpgrade;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f);

        Vector3 pos = hit.point;

        if (Input.GetKeyDown(KeyCode.U))
        {
            GameObject upgrade = Instantiate(pfUpgrade, pos + new Vector3(0f,1.5f,0f), Quaternion.identity);
        }
    }



    public bool generalDebug;
    public bool debugAi;
}
