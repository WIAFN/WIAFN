using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterMovement))]
public class PlayerMovementInput : MonoBehaviour
{
    private AudioManager audioManager;
    private CharacterMovement _characterMovement;

    // Start is called before the first frame update
    void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
       // audioManager = FindObjectOfType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _characterMovement.MovePlayer(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        PlayerActions();
    }

    private void PlayerActions()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _characterMovement.Jump();
            AudioManager.instance.PlayCharJump(transform);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _characterMovement.Dash();
            AudioManager.instance.PlayCharSlide(transform);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _characterMovement.StartSprinting();
            AudioManager.instance.PlayCharRun(transform);

        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _characterMovement.StopSprinting();
        }
    }
}
