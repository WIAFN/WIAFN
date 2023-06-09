using System.Collections.Generic;
using UnityEngine;

public class EffectBody : MonoBehaviour
{
    public string Name;
    public string Description;
    public List<Effect> Effects;
    public Color Color;
    public int Rarity;

    [Header("Utility")]
    public GameObject rotateTowardsPlayer;
    public EffectInformationPanel information;
    public ParticleSystem ps;


    private Animator _animator;
    private MeshRenderer _cubeX;
    private MeshRenderer _cubeY;
    private MeshRenderer _cubeZ;
    private Light _pointLight;
    private bool _isVisible;
    private bool _playerInRange;
    private bool _pickedUp;

    private void Awake()
    {
        OnCreate();
    }

    private void OnEnable()
    {
        PlayerCamera.OnInteract += OnPickup;
    }

    private void OnDisable()
    {
        PlayerCamera.OnInteract -= OnPickup;
    }

    private void Update()
    {
        rotateTowardsPlayer.transform.LookAt(GameManager.instance.mainPlayer.transform.position - Vector3.up * 1f);
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

        information.gameObject.SetActive(false);
        information.SetTexts(Name, Description);

        //Set colors of cubes + light, might make some interesting combinations
        SetColors();

        _pickedUp = false;

    }

    public void PlayerEnteredRange(Character player)
    {
        Debug.Log("Player in range " + gameObject.name);
        information.gameObject.SetActive(true);
        _playerInRange = true;
    }

    public void PlayerLeftRange(Character player)
    {
        Debug.Log("Player left range of " + gameObject.name);
        information.GetComponent<Animator>().SetTrigger("PlayerLeftRange");
        _playerInRange = false;
    }

    private void OnPickup()
    {
        if (CanCollect())
        {
            _animator.SetTrigger("PickUp");
            information.gameObject.SetActive(false);
            foreach(Effect effect in Effects) 
            {
                effect.OnEffectPickup(GameManager.instance.mainPlayer);
            }
            _pickedUp = true;
        }

    }

    private bool CanCollect()
    {
        return _isVisible && _playerInRange;
    }

    public void RefreshText()
    {
        information.SetTexts(Name, Description);
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

        var main = ps.main;
        var trails = ps.trails;
        if (Rarity > 2)
        {
            ps.gameObject.SetActive(true);
            // Weird unity stuff, doesnt work if its not variable

            main.startLifetime = Mathf.Clamp(1 + Mathf.Ceil((Rarity - 2) / 2f), 0f, 3f);

            Gradient gradient;
            GradientColorKey[] colorKey;
            GradientAlphaKey[] alphaKey;

            gradient = new Gradient();

            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            colorKey = new GradientColorKey[2];
            colorKey[0].color = Color;
            colorKey[0].time = 0.0f;
            colorKey[1].color = Color.white;
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 0.0f;
            alphaKey[1].time = 1.0f;

            gradient.SetKeys(colorKey, alphaKey);

            trails.colorOverLifetime = gradient;
        }
        else
        {
            main.startLifetime = 0;
        }

    }

    public bool PickedUp => _pickedUp;
}
