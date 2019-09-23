using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuickPlacer))]
public class QuickPlacerCustomInspector : Editor
{
    // For the proccess
    protected Transform transform;
    protected SerializedProperty prefabToSpawn;
    protected SerializedProperty randomRotation;
    protected GameObject instance;
    protected GameObject previousInstance;
    private int tempLayer;

    // Snap
    protected SerializedProperty snap;
   
    // Editor
    protected bool isEditing;
    protected string editStatus = "Not Editing";

    protected virtual void OnEnable()
    {
        prefabToSpawn = serializedObject.FindProperty("prefabToSpawn");
        randomRotation = serializedObject.FindProperty("randomRotation");
        snap = serializedObject.FindProperty("snap");
        SetTransform();
    }

    /// <summary>
    /// Gets the Transform of the inspected object. Must be overridden for each derived class.
    /// </summary>
    protected virtual void SetTransform()
    {
        QuickPlacer qp = (QuickPlacer)target;
        transform = qp.transform;
    }

    /// <summary>
    /// Draw the buttons and labels as well as the base fields.
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Place Mode"))
        {
            editStatus = PlaceMode();
        }
        else if (GUILayout.Button("Stop Editing"))
        {
            editStatus = StopEditing();
        }
        
        if(editStatus != "Not Editing")
            GUI.color = Color.yellow;
        GUILayout.Label("Edit Status: " + editStatus);
        GUI.color = Color.white;
    }

    /// <summary>
    /// If we're editing, do special things.
    /// </summary>
    protected void OnSceneGUI()
    {
        if (isEditing)
        {
            // Makes sure we dont select anything else while in "edit mode"
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            SendEvent();
        }
    }

    /// <summary>
    /// Set editor info for Placing.
    /// </summary>
    /// <returns></returns>
    protected virtual string PlaceMode()
    {
        isEditing = true;
        previousInstance = null;
        return "Placing";
    }

    /// <summary>
    /// Set editor info for no longer editing.
    /// </summary>
    /// <returns></returns>
    protected virtual string StopEditing()
    {
        isEditing = false;
        previousInstance = null;
        return "Not Editing";
    }

    /// <summary>
    /// Decide where to send the Event.
    /// </summary>
    protected virtual void SendEvent()
    {
        Event e = Event.current;

        // If the event involves the mouse and it's left button
        if (e.button == 0 && e.isMouse)
        {
            if (e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000))
                {
                    MouseDown(hit);
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && instance)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000))
                {
                    Drag(hit);
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                MouseUp();
                e.Use();
            }
        }
        else if (e.type == EventType.ScrollWheel)
        {
            if (instance)
                instance.transform.Rotate(instance.transform.up, e.delta.y * 1.5f);
            e.Use();
        }
        else if (e.type == EventType.Repaint)
        {
            DrawArrows();
        }
    }

    /// <summary>
    /// What to do on a MouseDown Event.
    /// </summary>
    /// <param name="hit">Raycast from scene camera</param>
    protected virtual void MouseDown(RaycastHit hit)
    {
        instance = SpawnInstance((GameObject)prefabToSpawn.objectReferenceValue);
        if (!instance)
            return;
        instance.transform.SetParent(transform);
        instance.transform.position = hit.point;
        if (randomRotation.boolValue)
            instance.transform.Rotate(transform.up, UnityEngine.Random.Range(0f, 360f));
        tempLayer = instance.layer;
        instance.layer = 2;
    }

    /// <summary>
    /// What to do on a MouseDrag Event.
    /// </summary>
    /// <param name="hit">Raycast from scene camera</param>
    protected virtual void Drag(RaycastHit hit)
    {
        if (snap.boolValue)
        {
            instance.transform.position = new Vector3(
                Mathf.Round(hit.point.x),
                Mathf.Round(hit.point.y),
                Mathf.Round(hit.point.z)
                );
        }
        else
            instance.transform.position = hit.point;
    }

    /// <summary>
    /// What to do on a MouseUp Event.
    /// </summary>
    protected virtual void MouseUp()
    {
        if (instance)
        {
            previousInstance = instance;
            instance.layer = tempLayer;
            Undo.RegisterCreatedObjectUndo(instance, "Place Object");
        }
        instance = null;
    }

    /// <summary>
    /// Draw Handle arrows point in all of the target's childs' forward directions.
    /// </summary>
    protected virtual void DrawArrows()
    {
        Handles.color = Handles.zAxisColor;
        // Only getting immediate children.
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = (transform).GetChild(i);
            Handles.ArrowHandleCap(0, child.transform.position, Quaternion.LookRotation(child.transform.forward), 5, EventType.Repaint);
        }
    }

    /// <summary>
    /// Spawn an instance in the scene. Takes into account if it's a prefab or not.
    /// </summary>
    /// <param name="original"></param>
    /// <returns>Spawned instance</returns>
    protected GameObject SpawnInstance(GameObject original)
    {
        if (!original)
            return null;

        GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(original);
        if (prefabRoot)
            return (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
        else
        {
            GameObject attemptedSpawn = (GameObject)PrefabUtility.InstantiatePrefab(original);
            if (attemptedSpawn)
                return attemptedSpawn;
            return Instantiate(original);
        }
    }
}
