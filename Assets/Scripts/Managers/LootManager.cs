using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Rendering;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;

    public float lootPercent;

    public GameObject pfUpgrade;

    public List<Upgrade> WeaponUpgrades;
    public List<Upgrade> CharacterUpgrades;
    public List<Upgrade> MovementUpgrades;
    public List<Effect> Boosts;

    private GameManager _gm;

    private readonly float LUCK_LOWER_BOUND = 30f;
    private readonly float LUCK_UPPER_BOUND = 80f;

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

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.L)) 
        {
            CreateLoot(GameManager.instance.mainPlayer.transform.position);
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            CreateLoot(GameManager.instance.mainPlayer.transform.position, true);
        }
    }

    private void Start()
    {
        _gm = GameManager.instance;
        _gm.OnCharacterDied += this.OnCharacterDied;

        List<Upgrade> upgrades = Resources.LoadAll<Upgrade>("Upgrades").ToList();
        Boosts = Resources.LoadAll<Effect>("Effects").ToList();
        WeaponUpgrades = new();
        CharacterUpgrades = new();
        MovementUpgrades = new();

        foreach(Upgrade upgrade in upgrades)
        {
            if (upgrade.name.StartsWith("W_")) WeaponUpgrades.Add(upgrade);
            if (upgrade.name.StartsWith("C_")) CharacterUpgrades.Add(upgrade);
            if (upgrade.name.StartsWith("M_")) MovementUpgrades.Add(upgrade);
        }
    }

    private void OnDestroy()
    {
        if (_gm != null)
        {
            _gm.OnCharacterDied -= this.OnCharacterDied;
        }
    }

    public void CreateLoot(Vector3 position, bool isBoost = false)
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
        

        if (isBoost)
        {
            AddBoost(upgrade);
        }
        else
        {
            RandomizeLoot(upgrade, GetRarity());
        }

    }

    public void RandomizeLoot(GameObject upgrade, int rarity)
    {
        Color color = Color.white;
        List<Upgrade> upgrades = new();
        EffectBody container = upgrade.GetComponent<EffectBody>();
        container.Rarity = rarity;
        Effect effect = (Effect)ScriptableObject.CreateInstance("Effect");

        int upgradeCount = 1 + (int)Mathf.Ceil(rarity / 2f);
        bool hasDetrimental = upgradeCount > 1 && rarity < 4;

        switch (GetUpgradeType())
        {
            case "Movement":
                upgrades = MovementUpgrades;
                color = Color.cyan;
                container.Name = "Movement Upgrade";
                break;
            case "Character":
                upgrades = CharacterUpgrades;
                color = Color.yellow;
                container.Name = "Character Upgrade";
                break;
            case "Weapon":
                upgrades = WeaponUpgrades;
                upgradeCount = Mathf.Clamp(upgradeCount, 0, 2);
                color = Color.red;
                container.Name = "Weapon Upgrade";
                if (hasDetrimental)
                {
                    upgradeCount = 1;
                }
                break;
        }

        while (effect.Upgrades.Count < upgradeCount)
        {
            float luck = (Random.value) * 100f;

            int index = Random.Range(0, upgrades.Count);
            Upgrade u = Instantiate(upgrades[index]);
            u.name = $"{u.name.Replace("(Clone)", "")}";

            if (CantExist(u,effect)) continue;

            if (luck > (LUCK_LOWER_BOUND - rarity) && luck < (LUCK_UPPER_BOUND + rarity))
            {
                u.Multiplier += Mathf.Round(Random.value * 10f) / 10f;
            }

            effect.Upgrades.Add(u);
        }

        while (hasDetrimental)
        {
            int index = Random.Range(0, upgrades.Count);
            Upgrade u = Instantiate(upgrades[index]);
            u.name = $"{u.name.Replace("(Clone)", "")}";

            if (CantExist(u, effect)) continue;

            u.IsDetrimental = true;
            effect.Upgrades.Add(u);
            hasDetrimental = false;
        }

        container.Effects.Add(effect);
        container.Color = color;
        container.SetColors();
        container.Description = GetDescription(effect);
        container.RefreshText();
    }

    public void AddBoost(GameObject upgrade)
    {
        EffectBody container = upgrade.GetComponent<EffectBody>();
        bool cantExist = true;
        Effect effect = (Effect)ScriptableObject.CreateInstance("Effect");
        while (cantExist)
        {
            int index = Random.Range(0, Boosts.Count);
            effect = Boosts[index];
            cantExist = effect.name.StartsWith("#_");
        }
        
        effect.Upgrades = new();
        container.Effects.Add(effect);
        container.Name = effect.name;
        container.Description = "TEMPORARY INCREASE OF STATS";
        container.RefreshText();
    }

    private string GetUpgradeType()
    {
        float percentage = Random.value * 100f;
        if (percentage < 25f) return "Movement";
        if (percentage < 65f) return "Character";
        return "Weapon";
    }

    private bool CantExist(Upgrade u, Effect effect)
    {
        bool exists = false;
        foreach (Upgrade up in effect.Upgrades)
        {
            if (up.name.Equals(u.name))
            {
                exists = true;
                break;
            }
        }
        //Exclude key items and Temp boosts
        return u.name.StartsWith("#_") || u.name.StartsWith("T_") || exists;
    }

    private string GetDescription(Effect effect)
    {
        string description = "";
        foreach(Upgrade u in effect.Upgrades)
        {
            string upgradeName = u.name.Substring(2);
            string status = u.IsDetrimental ? "DOWN" : "UP";
            description += upgradeName + "\t\t" + status + "\n";
        }
        return description;
    }

    private int GetRarity()
    {
        float percentage = Random.value * 100f;
        if (percentage < 10f) return 5;
        if (percentage < 25f) return 4;
        if (percentage < 45f) return 3;
        if (percentage < 65f) return 2;
        return 1;
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
