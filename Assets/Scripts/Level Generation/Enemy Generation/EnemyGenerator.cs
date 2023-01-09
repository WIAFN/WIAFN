using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(JunkyardLevelGenerator))]
public class EnemyGenerator : MonoBehaviour
{
    public int enemyCount;
    public List<GameObject> enemies;

    private JunkyardLevelGenerator _levelGenerator;

    private Transform _enemiesParent;

    private void Awake()
    {
        _levelGenerator = GetComponent<JunkyardLevelGenerator>();

        _enemiesParent = (new GameObject("Enemies")).transform;
        _enemiesParent.parent = transform.parent;

        _levelGenerator.OnGenerationCompleted += OnGenerationCompleted;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        _levelGenerator.OnGenerationCompleted -= OnGenerationCompleted;
    }

    public void OnGenerationCompleted()
    {
        if (!isActiveAndEnabled) {  return; }
        StartCoroutine(GenerateEnemies());
    }

    private IEnumerator GenerateEnemies()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = enemies[Random.Range(0, enemies.Count)];

            Vector3 worldPos = _levelGenerator.GenerateRandomPosition();
            worldPos.y = _levelGenerator.GetPileHeightAtWorldPos(worldPos.x, worldPos.z);
            //if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 100f, -1) && hit.position.y < worldPos.y + 10f)
            //{
                GameObject enemyInstance = Instantiate(enemy, worldPos/*hit.position*/, Quaternion.identity, _enemiesParent);
            //}

        }
    }
}
