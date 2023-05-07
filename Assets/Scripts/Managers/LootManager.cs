using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;

    public float lootPercent;

    public GameObject pfUpgrade;

    private GameManager _gm;

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

    private void Start()
    {
        _gm = GameManager.instance;
        _gm.OnCharacterDied += this.OnCharacterDied;
    }

    private void OnDestroy()
    {
        if (_gm != null)
        {
            _gm.OnCharacterDied -= this.OnCharacterDied;
        }
    }

    public void CreateLoot(Vector3 position)
    {
        Vector3 pos;
        Physics.Raycast(position, Vector3.up, out RaycastHit hit, 0.5f);
        pos = hit.point;
        if (hit.collider == null)
        {
            Physics.Raycast(position, Vector3.down, out hit, 5f);
            pos = hit.point;
            if (hit.collider == null)
            {
                Debug.LogWarning($"Created a loot at wrong position: {position}");
                pos = position;
            }

        }

        GameObject upgrade = Instantiate(pfUpgrade, pos + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
    }

    public void OnCharacterDied(Character character)
    {
        if (!character.CompareTag("Player"))
        {
            if (Random.value < lootPercent / 100f)
            {
                CreateLoot(character.transform.position);
            }
        }
    }
}
