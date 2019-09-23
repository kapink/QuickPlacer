using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HydroPolePlacer : QuickPlacer
{
    [SerializeField]
    private GameObject wirePrefab;
    [SerializeField]
    [Range(2, 20)]
    private int numberOfPoints = 3;
    [SerializeField]
    private AnimationCurve curve;
}
