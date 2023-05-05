using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WIAFN.LevelGeneration;

[RequireComponent(typeof(JunkyardLevelGenerator))]
public class EnemyGenerator : MonoBehaviour
{
    public int enemyCount;
    public List<GameObject> enemies;

    private JunkyardLevelGenerator _levelGenerator;

    private Transform _enemiesParent;
    private ItemPoolGenerator _itemPoolGenerator;

    private void Awake()
    {
        _levelGenerator = GetComponent<JunkyardLevelGenerator>();

        _enemiesParent = (new GameObject("Enemies")).transform;
        _enemiesParent.parent = transform.parent;

        _levelGenerator.OnGenerationCompleted += OnGenerationCompleted;
    }

    private void Start()
    {
        _itemPoolGenerator = new ItemPoolGenerator(enemyCount);
        _itemPoolGenerator.StartGeneration(this, GetRandomEnemy, true, _enemiesParent);
    }

    private void OnDestroy()
    {
        _levelGenerator.OnGenerationCompleted -= OnGenerationCompleted;

        _itemPoolGenerator?.DestroyItems();
        _itemPoolGenerator = null;
    }

    public void OnGenerationCompleted()
    {
        if (!isActiveAndEnabled) {  return; }
        StartCoroutine(MoveObjectsToAppropriatePositions());
    }

    private IEnumerator MoveObjectsToAppropriatePositions()
    {
        while (!_itemPoolGenerator.IsCompleted)
        {
            yield return null;
        }

        foreach(GameObject item in _itemPoolGenerator.Pool)
        {
            Vector3 worldPos = _levelGenerator.GenerateRandomPosition();
            worldPos.y = _levelGenerator.GetPileHeightAtWorldPos(worldPos.x, worldPos.z) + (item.GetComponent<Collider>().bounds.size.y / 2f);

            //if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 100f, -1) && hit.position.y < worldPos.y + 10f)
            //{
            item.transform.position = worldPos/*hit.position*/;
            //}

            item.SetActive(true);
        }
    }

    private GameObject GetRandomEnemy()
    {
        GameObject enemy = enemies[Random.Range(0, enemies.Count)];
        return enemy;
    }
}
