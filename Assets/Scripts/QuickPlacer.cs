using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QuickPlacer : MonoBehaviour
{
    [SerializeField]
    private GameObject prefabToSpawn;
    public bool snap;
    [SerializeField]
    private bool randomRotation;

    // Can potentially move these fields right into the editor script... but then what's the point of having this script?
}
