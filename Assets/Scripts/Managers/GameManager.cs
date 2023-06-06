using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Cyan;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private UniversalRendererData _forwardRenderer;

    public Character mainPlayer;
    public LevelGeneratorBase levelGenerator;

    public Canvas sceneUI;

    public event CharacterDelegate OnCharacterDied;

    private LevelGeneratorBase _oldLevelGenerator;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            mainPlayer.gameObject.SetActive(false);
            sceneUI.gameObject.SetActive(false);
            instance.SetLevelGenerator(levelGenerator);
            Destroy(this.gameObject);
        }

        _oldLevelGenerator = null;

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
        //playerPos.y = levelGenerator.GetLevelHeightAt(mainPlayer.transform.position) + mainPlayer.GetComponent<Collider>().bounds.size.y / 2f;
        playerPos.y = levelGenerator.GetLevelHeightAt(mainPlayer.transform.position) + 15f;
        mainPlayer.transform.position = playerPos;
    }

    private void SetLevelGenerator(LevelGeneratorBase levelGenerator)
    {
        _oldLevelGenerator = this.levelGenerator;
        this.levelGenerator = levelGenerator;

        var oldPos = _oldLevelGenerator ? _oldLevelGenerator.transform.parent.position : Vector3.zero;
        levelGenerator.transform.parent.position = oldPos + new Vector3(1000f, 0f, 0f);
    }

    public delegate void CharacterDelegate(Character character);
}
