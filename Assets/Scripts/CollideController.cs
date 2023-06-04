using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideController : MonoBehaviour
{
    public bool _isCaught = false;
    public string _isObstacle = "Obstacle";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_isObstacle) && other.tag != null && !other.CompareTag(""))
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




  






