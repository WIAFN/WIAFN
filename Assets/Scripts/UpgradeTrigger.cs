using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTrigger : MonoBehaviour
{
    private const float CounterDelta = 0.3f;

    private float _playerLeftCounter = 0f;
    private Upgrade _upgradeScript;
    private Character _player;

    private void Awake()
    {
        _upgradeScript = GetComponentInParent<Upgrade>();
    }

    private void Start()
    {
        _player = null;
        ResetTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.gameObject.GetComponent<Character>();
            Debug.Log(player.tag + " " + player.gameObject.name);
            ResetTimer();
            _upgradeScript.PlayerEnteredRange(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.gameObject.GetComponent<Character>();
            _player = player;
            _playerLeftCounter = 0f;
        }
    }

    private void Update()
    {
        if (_playerLeftCounter >= 0f)
        {
            _playerLeftCounter += Time.deltaTime;
        }

        if (_playerLeftCounter > CounterDelta)
        {
            _upgradeScript.PlayerLeftRange(_player);
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        _playerLeftCounter = -1f;
    }
}
