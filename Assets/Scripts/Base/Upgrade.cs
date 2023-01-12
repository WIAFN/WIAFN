using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public string Name;
    public string Description;
    public List<KeyValuePair<string, int>> Effects;
    public Color Color;


    private Animator _animator;
    private MeshRenderer _cubeX;
    private MeshRenderer _cubeY;
    private MeshRenderer _cubeZ;
    private Light _pointLight;
    private GameObject _information;
    private bool _isVisible;
    private bool _playerInRange;
    private bool _pickedUp;

    private void Awake()
    {
        OnCreate();
    }

    private void FixedUpdate()
    {
        if (CanCollect())
        {
            PlayerCamera.OnInteract += OnPickup;
        }
        else
        {
            PlayerCamera.OnInteract -= OnPickup;
        }
        transform.LookAt(GameManager.instance.mainPlayer.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        Character player = other.gameObject.GetComponent<Character>();
        Debug.Log(player.tag + " " + player.gameObject.name);
        if (player.CompareTag("Player"))
        {
            PlayerEnteredRange(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Character player = other.gameObject.GetComponent<Character>();
        if (player.CompareTag("Player"))
        {
            PlayerLeftRange(player);
        }
    }
    private void OnBecameVisible()
    {
        _isVisible = true;
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
    }

    private void OnCreate()
    {
        //get animator
        _animator = GetComponent<Animator>();

        //get rotating cubes and point light
        Transform cubeParent = transform.GetChild(0);
        _cubeX = cubeParent.transform.GetChild(0).GetComponent<MeshRenderer>();
        _cubeY = cubeParent.transform.GetChild(1).GetComponent<MeshRenderer>();
        _cubeZ = cubeParent.transform.GetChild(2).GetComponent<MeshRenderer>();
        _pointLight = cubeParent.transform.GetChild(3).GetComponent<Light>();

        //information panel, idk?
        _information = transform.GetChild(1).gameObject;
        _information.SetActive(false);

        //TODO how to implement this?
        //upgrade effects go here
        Effects = new List<KeyValuePair<string, int>>();

        //Set colors of cubes + light, might make some interesting combinations
        SetColors();

        //TODO search if this is the best way to create this 
        Effects.Add(new ("maxHealth",100));
        Effects.Add(new ("Damage",10));
        Effects.Add(new ("FireRate",1));

        _pickedUp = false;

    }

    private void PlayerEnteredRange(Character player)
    {
        Debug.Log("Player in range " + gameObject.name);
        _information.SetActive(true);
        _playerInRange = true;
    }

    private void PlayerLeftRange(Character player)
    {
        Debug.Log("Player left range of " + gameObject.name);
        _information.GetComponent<Animator>().SetTrigger("PlayerLeftRange");
        _playerInRange = false;
    }

    private void OnPickup()
    {
        if (!_pickedUp)
        {
            _animator.SetTrigger("PickUp");
            Debug.Log("Picked up " + gameObject.name + " is picked up: " + _pickedUp);
            foreach (KeyValuePair<string, int> effect in Effects)
            {
                Debug.Log(effect.Key + " attribute changed by " + effect.Value);
                GameManager.instance.mainPlayer.GetUpgrade(effect.Key, effect.Value);
            }
            _pickedUp = true;
        }

    }

    private bool CanCollect()
    {
        return _isVisible && _playerInRange;
    }
    public void SetColors()
    {
        _cubeX.material.color = Color;
        _cubeX.material.SetColor("_EmissionColor", Color);
        _cubeY.material.color = Color;
        _cubeY.material.SetColor("_EmissionColor", Color);
        _cubeZ.material.color = Color;
        _cubeZ.material.SetColor("_EmissionColor", Color);
        _pointLight.color = Color;
    }
}
