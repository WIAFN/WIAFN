using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Character mainPlayer;

    public event CharacterDelegate OnCharacterDied;

    private AudioManager audioManager;

    private void Awake()
    {

        audioManager = FindObjectOfType<AudioManager>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance?.PlayBackgroundGame(transform);
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

    public delegate void CharacterDelegate(Character character);
}
