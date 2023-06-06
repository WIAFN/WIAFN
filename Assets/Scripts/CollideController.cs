using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIAFN.Constants;

public class CollideController : MonoBehaviour
{
    public bool _isCaught = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(WIAFNTags.Obstacle))
        {
            _isCaught = true;
            other.GetComponent<Collider>().enabled = false;
            //do something
            Debug.Log("Player hit an obstacle!");
        }
    }

    public bool IsObjectCaught()
    {
        return _isCaught;
    }
}




  






