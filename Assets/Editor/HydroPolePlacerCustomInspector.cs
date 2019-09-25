using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HydroPolePlacer))]
public class HydroPolePlacerCustomInspector : QuickPlacerCustomInspector
{
    private bool wiresOnly;
    SerializedProperty wirePrefab;
    SerializedProperty numberOfPoints;
    SerializedProperty curve;
    SerializedProperty wireConnectingName;
    SerializedProperty wireConnectionRadius;

    protected override void OnEnable()
    {
        base.OnEnable();
        wirePrefab = serializedObject.FindProperty("wirePrefab");
        numberOfPoints = serializedObject.FindProperty("numberOfPoints");
        curve = serializedObject.FindProperty("curve");
        wireConnectingName = serializedObject.FindProperty("wireConnectingName");
        wireConnectionRadius = serializedObject.FindProperty("wireConnectionRadius");
    }

    /// <summary>
    /// Same as base, but with class names changed.
    /// </summary>
    protected new void SetTransform()
    {
        HydroPolePlacer hp = (HydroPolePlacer)target;
        transform = hp.transform;
    }

    /// <summary>
    /// Draws additional hydro spesific buttons
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        try { GUILayout.Label("Previous Link: " + previousInstance.name); }
        catch { GUILayout.Label("Previous Link: null"); }

        if (GUILayout.Button("Wires Only"))
        {
            editStatus = WiresOnly();
        }
        if (GUILayout.Button("Remove Broken Wires"))
        {
            DestroyBrokenWires();
        }
    }

    /// <summary>
    /// Just calling base.
    /// </summary>
    private new void OnSceneGUI()
    {
        base.OnSceneGUI();
    }

    /// <summary>
    /// Place Mode
    /// </summary>
    /// <returns></returns>
    protected override string PlaceMode()
    {
        wiresOnly = false;
        return base.PlaceMode();
    }

    /// <summary>
    /// Wires only Edit mode
    /// </summary>
    /// <returns></returns>
    private string WiresOnly()
    {
        isEditing = true;
        wiresOnly = true;
        previousInstance = null;
        return "Wires Only";
    }

    /// <summary>
    /// Draws the wires between the poles
    /// </summary>
    protected override void MouseUp()
    {
        // NOTE that if it's wire only mode, wires are draw in mouse down. This is safe because it immediately sets the previous point to null when done drawing.
        if (instance && previousInstance)
            DrawWires(previousInstance, instance);

        base.MouseUp();
    }

    /// <summary>
    /// Checks if we should spawn an object, or if we are selecting something as the "previous instance"
    /// </summary>
    /// <param name="hit"></param>
    protected override void MouseDown(RaycastHit hit)
    {
        List<Transform> children = new List<Transform>();
        hit.transform.GetComponentsInChildren(children);
        GameObject linkableObject = children.Exists(child => child.name.Contains(wireConnectingName.stringValue)) ? hit.collider.gameObject : null;

        // If we click on a hydro pole (or something with [WirePoint]), dont spawn anobject,... do this instead.
        if (linkableObject)
        {
            // If we have a previous point in memory
            if (wiresOnly && previousInstance)
            {
                DrawWires(previousInstance, linkableObject);
                previousInstance = null;
            }
            else
                previousInstance = linkableObject;
        }
        else
            base.MouseDown(hit);
    }

    /// <summary>
    /// Using LineRenderers to draw wires between two game objects.
    /// </summary>
    /// <param name="start">game object that will parent the wires</param>
    /// <param name="end">game object to draw to</param>
    private void DrawWires(GameObject start, GameObject end)
    {
        // Get the wire points from the previous post
        List<Transform> previousPoints = new List<Transform>();
        start.GetComponentsInChildren(previousPoints);
        previousPoints.RemoveAll(children => !children.name.Contains(wireConnectingName.stringValue));

        // ...and the same for the recently placed pole
        List<Transform> currentPoints = new List<Transform>();
        end.GetComponentsInChildren(currentPoints);
        currentPoints.RemoveAll(children => !children.name.Contains(wireConnectingName.stringValue));

        for (int i = 0; i < previousPoints.Count; i++)
        {
            if (currentPoints.Count - 1 < i)
                break;

            GameObject wire = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)wirePrefab.objectReferenceValue);
            wire.transform.SetParent(previousPoints[i]);
            wire.transform.localPosition = Vector3.zero;
            wire.transform.rotation = previousPoints[i].rotation;
            LineRenderer line = wire.GetComponent<LineRenderer>();
            line.positionCount = numberOfPoints.intValue;

            // Calculate the points between the start and end points, and use the animation curve to figure out the dip.
            for (int j = 0; j < numberOfPoints.intValue; j++)
            {
                float pointNormal = Mathf.InverseLerp(0, numberOfPoints.intValue - 1, j);
                Vector3 relativeDestinationPoint = previousPoints[i].InverseTransformPoint(currentPoints[i].position);
                Vector3 pos = Vector3.Lerp(Vector3.zero, relativeDestinationPoint, pointNormal);
                pos.y += curve.animationCurveValue.Evaluate(pointNormal);
                line.SetPosition(j, pos);
            }

            Undo.RegisterCreatedObjectUndo(wire, "Create Hydro Wire");
        }
    }

    /// <summary>
    /// Looks at all the wire points and line beginning and end points. 
    /// If any line beginning or end point is too far from a wire point, the line is deleted.
    /// </summary>
    public void DestroyBrokenWires()
    {
        // Get all wire points
        List<Transform> wirePoints = new List<Transform>();
        transform.GetComponentsInChildren(wirePoints);
        wirePoints.RemoveAll(point => !point.name.Contains(wireConnectingName.stringValue));

        // Get all line points
        List<LineRenderer> lines = new List<LineRenderer>();
        transform.GetComponentsInChildren(lines);

        // Find out if the line start and end point are too far from a[WirePoint].
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (LineRenderer line in lines)
        {
            bool startPointClose = false;
            bool endPointClose = false;
            // Compare this line's start and end points to all the [WirePoint]s
            foreach (Transform wirePoint in wirePoints)
            {
                Vector3 wirePointLocalToWire = line.transform.InverseTransformPoint(wirePoint.position);
                // Start point 
                // NOTE: Start point may be redundant. The start point will move with the wirepoint and be deleted with the parent. So it will always be true.
                if (!startPointClose)
                {
                    float distance = Vector3.Distance(line.GetPosition(0), wirePointLocalToWire);
                    if (distance < wireConnectionRadius.floatValue)
                        startPointClose = true;
                }

                //End point
                if (!endPointClose)
                {
                    float distance = Vector3.Distance(line.GetPosition(line.positionCount - 1), wirePointLocalToWire);
                    if (distance < wireConnectionRadius.floatValue)
                        endPointClose = true;
                }
            }

            if (startPointClose && endPointClose)
                ; // TODO: Redraw wires. Need to store the start and end points and make a new method.
            else
                toDestroy.Add(line.gameObject); //Destroy
        }
        
        // Weird that the Undo is just a single undo. Seems like I'd need to press undo for each destroied object.
        toDestroy.ForEach(line => Undo.DestroyObjectImmediate(line));
    }

}
