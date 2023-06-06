using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioClip char_gunshot;
    public AudioClip enemy_gunshot;

    public AudioClip melee_sound;

    public AudioClip metalic_sound;

    public AudioClip char_slow_walk;
    public AudioClip char_run;
    public AudioClip char_slide;
    public AudioClip char_jump;

    public AudioClip enemy_walk;
    public AudioClip enemy_run;
    public AudioClip enemy_random;

    public AudioClip enemy_death;
    public AudioClip char_death;

    public AudioClip background_game;
    public AudioClip background_ui;


    private Dictionary<Transform, AudioSource> audioDict;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        audioDict = new Dictionary<Transform, AudioSource>();
    }

    public void Start()
    {
        audioDict.Clear();
    }

    public AudioSource getSource(Transform t)
    {
        AudioSource newSource;
        if (!audioDict.ContainsKey(t))
        {
            newSource = t.gameObject.AddComponent<AudioSource>();
            newSource.volume = 0.1f;
            audioDict.Add(t, newSource);
        }
        else
        {
            newSource = audioDict[t];
        }
        return newSource;
    }

    public void PlayCharGunshot(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(enemy_gunshot);
    }

    public void PlayEnemyGunshot(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(char_gunshot);
    }

    public void PlayMeleeSound(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(melee_sound);
    }

    public void PlayMetalicSound(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(metalic_sound);
    }

    public void PlayCharSlowWalk(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(char_slow_walk);
    }

    public void PlayCharRun(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(char_run);
    }

    public void PlayCharSlide(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(char_slide);
    }

    public void PlayCharJump(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(char_jump);
    }

    public void PlayEnemyWalk(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(enemy_walk);
    }

    public void PlayEnemyRun(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(enemy_run);
    }

    public void PlayEnemyRandom(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(enemy_random);
    }

    public void PlayEnemyDeath(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(enemy_death);
    }

    public void PlayCharDeath(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(char_death);
    }

    public void PlayBackgroundGame(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(background_game);
    }

    public void PlayBackgroundUI(Transform t)
    {
        var audioSource = getSource(t);
        audioSource.PlayOneShot(background_ui);
    }
}
