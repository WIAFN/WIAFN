using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Cyan;
using System;
using System.Linq;

[Serializable]
public class LevelInfo
{
    public int Level;
    public GameObject LevelObject;
    public LevelGeneratorBase Generator;

    public GameObject ElevatorOut;
    public GameObject ElevatorIn;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private UniversalRendererData _forwardRenderer;

    public bool levelGameManager;

    public Character mainPlayer;
    public List<LevelInfo> levelInfos;

    [Header("Prefabs")]
    public GameObject pfLevel;
    public GameObject pfElevatorOut;
    public GameObject pfElevatorIn;
    public GameObject pfPlayerFallChamber;

    public Canvas sceneUI;

    public LevelInfo CurrentLevelInfo { get { return levelInfos.Last(); } }
    public LevelGeneratorBase CurrentLevelGenerator { get { return CurrentLevelInfo.Generator; } }

    public event CharacterDelegate OnCharacterDied;
    public event LevelChangeDelegate OnLevelChanged;

    private AudioManager audioManager;

    private readonly int LEVEL_DESTROY_TIMER = 7;
    private readonly float LEVEL_GEN_DISTANCE = 1000f;

    private void Awake()
    {

        audioManager = FindObjectOfType<AudioManager>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            mainPlayer.gameObject.SetActive(false);
            sceneUI.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        ClearBlits();
        AudioManager.instance?.PlayBackgroundGame(transform);

        Debug.Assert(levelInfos.Count > 0);
        var levelInfo = CurrentLevelInfo;
        if (levelGameManager && levelInfo.ElevatorIn == null)
        {
            var elevatorInPos = levelInfo.Generator ? levelInfo.Generator.GenerateRandomPositionOnGround(true) : levelInfo.LevelObject.transform.position;
            levelInfo.ElevatorIn = CreateElevatorIn(levelInfo.LevelObject, elevatorInPos);
        }

        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitUntil(() => CurrentLevelInfo.Generator == null || CurrentLevelInfo.Generator.GenerationComplete);
        yield return null;

        if (OnLevelChanged != null)
        {
            LevelInfo lastLevelInfo = null;
            foreach (LevelInfo levelInfo in levelInfos)
            {
                OnLevelChanged(lastLevelInfo, levelInfo);
                lastLevelInfo = levelInfo;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // TODO - Safa: We should track characters using their OnDied event.
    public void CharacterDied(Character character)
    {
        AudioManager.instance?.PlayCharDeath(transform);

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

    internal LevelInfo CreateNewLevel(bool startGeneration = false)
    {
        var lastLevelInfo = CurrentLevelInfo;
        var newLevelInfo = new LevelInfo();
        newLevelInfo.Level = lastLevelInfo.Level + 1;
        var newLevel = Instantiate(pfLevel, lastLevelInfo.LevelObject.transform.position + Vector3.right * LEVEL_GEN_DISTANCE, Quaternion.identity);
        newLevel.name = "Level " + newLevelInfo.Level;
        newLevel.SetActive(startGeneration);
        newLevelInfo.LevelObject = newLevel;
        newLevelInfo.Generator = newLevel.GetComponentInChildren<LevelGeneratorBase>();

        // Creating next level object
        Vector3 elevatorOutPos = newLevelInfo.Generator.GenerateRandomPositionOnGround(towardsMiddle: true) + Vector3.up * 3f;
        Vector3 elevatorInPos = newLevelInfo.Generator.GenerateRandomPositionOnGround(towardsMiddle: true) + Vector3.up * 3f;

        // Creating elevator out object
        newLevelInfo.ElevatorOut = Instantiate(pfElevatorOut, elevatorOutPos, Quaternion.identity, newLevelInfo.LevelObject.transform);

        // Creating elevator in object
        newLevelInfo.ElevatorIn = CreateElevatorIn(newLevelInfo.LevelObject, elevatorInPos);

        return newLevelInfo;
    }

    internal void SetLevel(LevelInfo levelInfo)
    {
        var oldLevel = CurrentLevelInfo;
        this.levelInfos.Add(levelInfo);

        if (OnLevelChanged != null)
        {
            OnLevelChanged(oldLevel, levelInfo);
        }
    }

    internal void DestroyOldLevel(GameObject levelObject)
    {
        Destroy(levelObject, LEVEL_DESTROY_TIMER);
    }


    private GameObject CreateElevatorIn(GameObject levelObject, Vector3 elevatorInPos)
    {
        var elevatorIn = Instantiate(pfElevatorIn, elevatorInPos, Quaternion.identity, levelObject.transform);

        // Setting next elevator level
        var inController = elevatorIn.GetComponent<ElevatorController>();
        inController.CurrentLevel = levelObject;

        return elevatorIn;
    }

    public GameObject CreatePlayerFallChamber(Vector3 fallChamberPos)
    {
        return Instantiate(pfPlayerFallChamber, fallChamberPos, Quaternion.identity);
    }

    public delegate void CharacterDelegate(Character character);
    public delegate void LevelChangeDelegate(LevelInfo oldLevel, LevelInfo currentLevel);
}
