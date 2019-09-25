using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HydroPolePlacer : QuickPlacer
{
    [SerializeField]
    private GameObject wirePrefab;
    [SerializeField]
    private string wireConnectingName = "[WirePoint]";
    [SerializeField]
    private float wireConnectionRadius = 0.2f;
    [SerializeField]
    [Range(2, 20)]
    private int numberOfPoints = 3;
    [SerializeField]
    private AnimationCurve curve;

    private void OnDrawGizmosSelected()
    {
        List<Transform> children = new List<Transform>();
        transform.GetComponentsInChildren(children);
        List<Transform> wirePoints = children.FindAll(child => child.name.Contains(wireConnectingName));

        Gizmos.color = Color.yellow;
        wirePoints.ForEach(point => Gizmos.DrawWireSphere(point.position, wireConnectionRadius));
    }
}
