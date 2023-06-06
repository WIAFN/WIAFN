using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelElevator : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Starting level load");
            StartCoroutine(StartLoadAsync());
        }
    }
    IEnumerator StartLoadAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Level Generation", LoadSceneMode.Additive);

        //asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress);
            yield return null;
        }
        Debug.Log("Load done");
        LevelGeneratorBase levelGenerator = GameObject.Find("Generators").GetComponent<JunkyardLevelGenerator>();
        levelGenerator.OnGenerationCompleted += LevelGenerationComplete;
    }

    private void LevelGenerationComplete()
    {
        GameObject startingPoint = GameObject.Find("ElevatorIn");
        GameManager.instance.mainPlayer.transform.position = startingPoint.transform.position;
        startingPoint.name = "ElevatorOut";
        Debug.Log("Generation Complete");
    }

}
