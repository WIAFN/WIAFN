using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionController : MonoBehaviour
{
    public MouseLook mouseLook;
    public Slider sensSlider;
    public Slider snapSlider;
    public GameObject screen;

    // Start is called before the first frame update
    void Start()
    {
        mouseLook = GameManager.instance.mainPlayer.GetComponentInChildren<MouseLook>();
        sensSlider.value = mouseLook.mouseSensivity / 1000f;
        snapSlider.value = mouseLook.Snappiness / 100f;
    }

    public void Activate(bool IsLocked)
    {
        screen.SetActive(!IsLocked);
    }

    public void OnValueChanged()
    {
        mouseLook.mouseSensivity = sensSlider.value * 1000f;
        mouseLook.Snappiness = snapSlider.value * 100f;

    }
}
