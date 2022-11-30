using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterMovement))]
public class PlayerMovementInput : MonoBehaviour
{
    private CharacterMovement _characterMovement;

    // Start is called before the first frame update
    void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
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
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _characterMovement.Dash();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _characterMovement.StartSprinting();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _characterMovement.StopSprinting();
        }
    }
}
