using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorConsole : MonoBehaviour
{
    public int RequiredKeys;
    public Animator DoorAnimator;
    private bool _inRange;
    // Start is called before the first frame update
    void Start()
    {
        _inRange = false;
        PlayerCamera.OnInteract += ConsoleInteract;
    }

    private void ConsoleInteract()
    {
        if (_inRange)
        {
            int keyItemCount = GameManager.instance.mainPlayer.keyItems;
            // >= because bugs :D
            if(keyItemCount >= RequiredKeys)
            {
                DoorAnimator.SetTrigger("DoorOpen");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.gameObject.GetComponent<Character>();
            Debug.Log(player.keyItems);
            _inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.gameObject.GetComponent<Character>();
            _inRange = false;
        }
    }
}
