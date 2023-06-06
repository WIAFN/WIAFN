using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WIAFN.Constants;

public class LevelElevator : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Starting level load");
            StartCoroutine(StartLoadAsync());
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LevelGeneratorBase levelGenerator = GameManager.instance.levelGenerator;
            switch (levelGenerator.GenerationSpeed)
            {
                case LevelGenerationSpeed.Slow:
                    levelGenerator.GenerationSpeed = LevelGenerationSpeed.Fast;
                    Debug.Log("Set the level generation to fast.");
                    break;
                case LevelGenerationSpeed.Fast:
                    levelGenerator.GenerationSpeed = LevelGenerationSpeed.Slow;
                    Debug.Log("Set the level generation to slow.");
                    break;
                default:
                    Debug.LogAssertion("Level generation speed is invalid.");
                    break;
            }
        }
    }
    IEnumerator StartLoadAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(WIAFNScenes.Level0, LoadSceneMode.Additive);

        //asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress);
            yield return null;
        }
        Debug.Log("Load done");
        GameManager.instance.levelGenerator.OnGenerationCompleted += LevelGenerationComplete;
        GameManager.instance.levelGenerator.GenerationSpeed = LevelGenerationSpeed.Slow;
    }

    private void LevelGenerationComplete()
    {
        Vector3 startingPoint = GameManager.instance.levelGenerator.GenerateRandomPositionOnGround(towardsMiddle: true) + Vector3.up * 3f;
        GameManager.instance.mainPlayer.transform.position = startingPoint;
        Debug.Log("Generation Complete");
    }

}
