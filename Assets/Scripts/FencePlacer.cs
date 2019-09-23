using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FencePlacer : QuickPlacer
{
    [SerializeField]
    private Material fenceMaterial;
    [SerializeField]
    [Range(0,1)]
    private float heightPercent = 1;
}
