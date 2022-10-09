using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCrosshair : MonoBehaviour
{
    public Color color;
    public void ChangeColor()
    {
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Cross"))
        {
            go.GetComponent<Image>().color = color;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ChangeColor();
        }    
    }
}
