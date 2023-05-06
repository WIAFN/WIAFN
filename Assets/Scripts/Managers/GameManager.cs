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
    public LevelGeneratorBase levelGenerator;

    public Canvas sceneUI;

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

        if (levelGenerator != null)
        {
            levelGenerator.OnGenerationCompleted += LevelGenerator_OnGenerationCompleted;
        }
    }

    public void OnDestroy()
    {
        if (levelGenerator != null)
        {
            levelGenerator.OnGenerationCompleted -= LevelGenerator_OnGenerationCompleted;
        }

        ClearBlits();
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

    //Turn all fullscreen effects off when destroyed to prevent them from reoccuring next the game launches
    private void ClearBlits()
    {
        foreach(ScriptableRendererFeature feature in _forwardRenderer.rendererFeatures)
        {
            //Doing a check incase we add something non blit to the forward renderer
            if(feature.GetType() == typeof(Blit)) 
            {
                Blit blit = (Blit)feature;

                //Setting intensity to 0 so it doesnt lerp from 1 to 0 instantly when called
                blit.settings.blitMaterial.SetFloat("_Intensity", 0);
                blit.SetActive(false);
            }
        }
    }

    private void LevelGenerator_OnGenerationCompleted()
    {
        Vector3 playerPos = mainPlayer.transform.position;
        playerPos.y = levelGenerator.GetLevelHeightAtWorldPos(mainPlayer.transform.position);
        mainPlayer.transform.position = playerPos;
    }

    public delegate void CharacterDelegate(Character character);
}
