using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager instance;

    public GameObject pfElevatorKey;

    public List<GameObject> hasPresetKeys;

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
            for (int i = 0; i < 10; i++)
            {
                Vector3 keyPosition = currentLevel.Generator != null ? currentLevel.Generator.GenerateRandomPositionOnGround() : currentLevel.ElevatorIn.transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                keyPosition += Vector3.up * 1.5f;
                GameObject newKey = Instantiate(pfElevatorKey, keyPosition, Quaternion.identity);
                newKey.transform.parent = currentLevel.LevelObject.transform;
            }
        }
    }
}
