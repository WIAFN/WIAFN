using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorBase : MonoBehaviour
{
    public Vector3 levelSizeInMeters;

    public Vector3 HalfLevelSizeInMeters
    {
        get
        {
            return levelSizeInMeters / 2;
        }
    }
}
