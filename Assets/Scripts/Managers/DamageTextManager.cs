using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public GameObject _damageTextPrefab;

    public Color lowDamage;
    public Color mediumDamage;
    public Color highDamage;
    public Color specialColor;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.OnDamage += OnDamage;
    }

    private void OnDestroy()
    {
        if(GameManager.instance != null)
        {
            GameManager.instance.OnDamage -= OnDamage;
        }
    }

    public void OnDamage(Character from, Character to, float damage)
    {
        var mainPlayer = GameManager.instance.mainPlayer;
        if (to != mainPlayer)
        {
            Vector3 pos = to.transform.position + Vector3.up * UnityEngine.Random.value; 
            GameObject obj = Instantiate(_damageTextPrefab, pos, Quaternion.identity);
            obj.transform.LookAt(mainPlayer.transform);
            Destroy(obj, 1.5f);

            TextMeshPro textMesh = obj.GetComponentInChildren<TextMeshPro>();
            if (damage > 1)
            {
                textMesh.text = Mathf.FloorToInt(damage).ToString();
            }
            else
            {
                textMesh.text = Math.Round(damage, 1).ToString();
            }
            float oldAlpha = textMesh.alpha;
            if (from == mainPlayer)
            {
                if (from.Effect != null && from.Effect.Enabled)
                {
                    textMesh.color = specialColor;
                }
                else if (damage >= 20)
                {
                    textMesh.color = highDamage;
                }
                else if (damage >= 10)
                {
                    textMesh.color = mediumDamage;
                }
                else
                {
                    textMesh.color = lowDamage;
                }

            }
            else
            {
                textMesh.color = Color.gray;
            }

            textMesh.alpha = oldAlpha;
        }
    }
}
