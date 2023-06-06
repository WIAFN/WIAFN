using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WIAFN.LevelGeneration;

[RequireComponent(typeof(LevelGeneratorBase))]
public class EnemyGenerator : MonoBehaviour
{
    public int enemyCount;
    public List<GameObject> enemies;

    private LevelGeneratorBase _levelGenerator;

    private Transform _enemiesParent;
    private ItemPoolGenerator _itemPoolGenerator;

    private void Awake()
    {
        _levelGenerator = GetComponent<LevelGeneratorBase>();

        _enemiesParent = (new GameObject("Enemies")).transform;
        _enemiesParent.parent = transform.parent;
        _enemiesParent.transform.localPosition = Vector3.zero;

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
            Vector3 worldPos = _levelGenerator.GenerateRandomPositionOnLevel();

            item.transform.position = worldPos;

            item.SetActive(true);

            worldPos.y += (item.GetComponent<Collider>().bounds.size.y / 2f);
            //if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 100f, -1) && hit.position.y < worldPos.y + 10f)
            //{
            item.transform.position = worldPos/*hit.position*/;
            //}
        }

    }

    private GameObject GetRandomEnemy()
    {
        GameObject enemy = enemies[Random.Range(0, enemies.Count)];
        return enemy;
    }
}
