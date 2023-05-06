using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WIAFN.Constants;

public class LoadingScreenScript : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI randomFactsText;

    public float loadingDotChangeDeltaTime;
    public float factChangeDeltaTime;

    public string[] facts;

    private bool _willChangeScene;
    private AsyncOperation _sceneChangeAsyncOp;
    private GameManager _newLevelGameManager;

    private float _timePassed;
    private float _loadingDotTargetTime;
    private float _factChangeTargetTime;

    private System.Random _factsRng;

    private void Awake()
    {
        _newLevelGameManager = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        _timePassed = 0f;

        _willChangeScene = GameManager.instance == null;
        if (_willChangeScene)
        {
            _sceneChangeAsyncOp = SceneManager.LoadSceneAsync(WIAFNScenes.Level0, LoadSceneMode.Additive);
        }
        else
        {
            _sceneChangeAsyncOp = null;
        }

        _factsRng = new System.Random();

        ChangeFactText();
    }

    private void OnDestroy()
    {
        if (_newLevelGameManager != null)
        {
            if (_newLevelGameManager.sceneUI != null)
            {
                _newLevelGameManager.sceneUI.enabled = true;
            }

            if (_newLevelGameManager.levelGenerator != null)
            {
                _newLevelGameManager.levelGenerator.OnGenerationCompleted -= OnLevelGenerationCompleted;
            }
        }
        _newLevelGameManager = null;
    }

    // Update is called once per frame
    void Update()
    {
        _timePassed += Time.deltaTime;

        if (_willChangeScene && _sceneChangeAsyncOp.isDone && _newLevelGameManager == null)
        {
            Camera.main.gameObject.SetActive(false);
            _newLevelGameManager = GameManager.instance;
            _newLevelGameManager.sceneUI.enabled = false;

            if (_newLevelGameManager.levelGenerator != null)
            {
                _newLevelGameManager.levelGenerator.OnGenerationCompleted += OnLevelGenerationCompleted;
                _newLevelGameManager.levelGenerator.GenerationSpeed = LevelGenerationSpeed.Fast;
            }
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
        string newText = text.Substring(0, text.Length - dotCount) + (new string('.', newDotCount));
        loadingText.text = newText;

        return newText;
    }

    private string ChangeFactText()
    {
        string newFact = facts[_factsRng.Next(facts.Length)];
        randomFactsText.text = newFact;
        return newFact;
    }

    private void OnLevelGenerationCompleted()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
    }
}
