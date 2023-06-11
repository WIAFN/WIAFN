using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LootWindowScript : MonoBehaviour
{
    public TextMeshProUGUI _keysText;
    public TextMeshProUGUI _effectText;

    private bool _changeHappened;


    // Start is called before the first frame update
    void Start()
    {
        _changeHappened = true;

        PlayerCamera.OnInteract += OnInteract;
    }

    private void OnDestroy()
    {
        PlayerCamera.OnInteract -= OnInteract;
    }

    private void OnInteract()
    {
        _changeHappened = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_changeHappened)
        {
            UpdateLoots();
            _changeHappened = false;
        }
    }

    private void UpdateLoots()
    {
        if (GameManager.instance != null)
        {
            var player = GameManager.instance.mainPlayer;
            LevelInfo levelInfo = GameManager.instance.CurrentLevelInfo;
            var console = levelInfo.ElevatorIn.GetComponentInChildren<ElevatorConsole>();
            _keysText.text = $"KEYS {player.keyItems}/{(console != null ? console.RequiredKeys: -1)}";
            _effectText.text = player.Effect? player.Effect.name: "No Effect";
        }
    }
}
