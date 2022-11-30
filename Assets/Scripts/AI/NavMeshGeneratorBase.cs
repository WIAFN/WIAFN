using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshGeneratorBase : MonoBehaviour
{
    public float navMeshUpdateInterval;

    public LayerMask includeLayers;
}
