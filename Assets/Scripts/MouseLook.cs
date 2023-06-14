using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensivity = 2000f;
    public Transform playerBody;
    public float xRotation = 0f;
    public bool IsLocked = false;
    private float xAccumulator;
    private float yAccumulator;
    public float Snappiness = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        IsLocked= true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IsLocked = !IsLocked;
            Cursor.lockState = IsLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !IsLocked;
        }
        if(IsLocked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;

            xAccumulator = Mathf.Lerp(xAccumulator, mouseX, Snappiness * Time.deltaTime);
            yAccumulator = Mathf.Lerp(yAccumulator, mouseY, Snappiness * Time.deltaTime);

            xRotation -= yAccumulator;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * xAccumulator);
        }
    }

    public void SetCameraRotation(Quaternion quaternion)
    {
        Vector3 euler = quaternion.eulerAngles;
        xRotation = euler.x;

        playerBody.Rotate(Vector3.up * euler.y);
    }
}
