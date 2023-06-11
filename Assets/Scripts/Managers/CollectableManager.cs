using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager instance;

    public GameObject pfElevatorKey;

    public List<GameObject> hasPresetKeys;

    public int keyCountPerLevel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.OnLevelChanged += OnLevelChanged;
    }

    private void Update()
    {

        if (Input.GetKeyUp(KeyCode.M))
        {
            CreateKey(GameManager.instance.CurrentLevelInfo, GameManager.instance.mainPlayer.transform.position);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnLevelChanged -= OnLevelChanged;
        }
    }

    private void OnLevelChanged(LevelInfo oldLevel, LevelInfo currentLevel)
    {
        CreateElevatorKeys(currentLevel);
    }

    private void CreateElevatorKeys(LevelInfo currentLevel)
    {
        if (hasPresetKeys.Contains(currentLevel.LevelObject)) return;

        if (currentLevel.ElevatorIn != null)
        {
            for (int i = 0; i < keyCountPerLevel; i++)
            {
                Vector3 keyPosition = currentLevel.Generator != null ? currentLevel.Generator.GenerateRandomPositionOnGround() : currentLevel.ElevatorIn.transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                CreateKey(currentLevel, keyPosition);
            }
        }
    }

    private void CreateKey(LevelInfo currentLevel, Vector3 keyPosition)
    {
        keyPosition += Vector3.up * 1.5f;
        GameObject newKey = Instantiate(pfElevatorKey, keyPosition, Quaternion.identity);
        newKey.transform.parent = currentLevel.LevelObject.transform;
    }
}
