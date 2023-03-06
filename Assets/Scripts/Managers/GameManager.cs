using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Cyan;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private UniversalRendererData _forwardRenderer;
    public static GameManager instance;

    public Character mainPlayer;

    public event CharacterDelegate OnCharacterDied;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO - Safa: We should track characters using their OnDied event.
    public void CharacterDied(Character character)
    {
        OnCharacterDied?.Invoke(character);
    }

    public void OnDestroy()
    {
        ClearBlits();
    }

    //Turn all fullscreen effects off when destroyed to prevent them from reoccuring next the game launches
    private void ClearBlits()
    {
        foreach(ScriptableRendererFeature feature in _forwardRenderer.rendererFeatures)
        {
            //Doing a check incase we add something non blit to the forward renderer
            if(feature.GetType() == typeof(Blit)) 
            {
                feature.SetActive(false);
            }
        }
    }
    public delegate void CharacterDelegate(Character character);
}
