using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    public bool generalDebug;
    public bool debugAi;
    public bool debugProceduralAnims;
    public int LeanTweenCallCount;

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
    }

    private void Update()
    {
        LeanTweenCallCount = LeanTween.tweensRunning;
    }

}
