using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenScript : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI randomFactsText;

    public float loadingDotChangeDeltaTime;
    public float factChangeDeltaTime;

    public string[] facts;

    private bool _willChangeScene;
    private AsyncOperation _sceneChangeAsyncOp;

    private float _timePassed;
    private float _loadingDotTargetTime;
    private float _factChangeTargetTime;

    private System.Random _factsRng;

    // Start is called before the first frame update
    void Start()
    {
        _timePassed = 0f;

        _willChangeScene = GameManager.instance == null;
        if (_willChangeScene)
        {
            _sceneChangeAsyncOp = SceneManager.LoadSceneAsync("Level Generation");
        }
        else
        {
            _sceneChangeAsyncOp = null;
        }

        _factsRng = new System.Random();

        ChangeFactText();
    }

    // Update is called once per frame
    void Update()
    {
        _timePassed += Time.deltaTime;

        if (_willChangeScene && _sceneChangeAsyncOp.isDone)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
            return;
        }

        if (_timePassed >= _loadingDotTargetTime)
        {
            _loadingDotTargetTime = _timePassed + loadingDotChangeDeltaTime;
            ChangeDotText();
        }

        if (_timePassed >= _factChangeTargetTime)
        {
            _factChangeTargetTime = _timePassed + factChangeDeltaTime;
            ChangeFactText();
        }
    }

    private string ChangeDotText()
    {
        string text = loadingText.text;
        int dotCount = text.Count((chara) => { return chara == '.'; });
        int newDotCount = (dotCount % 3) + 1;
        string newText = text.Substring(0, text.Length - newDotCount + 1) + (new string('.', newDotCount));
        loadingText.text = newText;

        return newText;
    }

    private string ChangeFactText()
    {
        string newFact = facts[_factsRng.Next(facts.Length)];
        randomFactsText.text = newFact;
        return newFact;
    }
}
