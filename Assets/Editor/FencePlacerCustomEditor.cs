using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FencePlacer))]
public class FencePlacerCustomEditor : QuickPlacerCustomInspector
{
    SerializedProperty fenceMat;
    SerializedProperty heightPercent;

    protected override void OnEnable()
    {
        fenceMat = serializedObject.FindProperty("fenceMaterial");
        heightPercent = serializedObject.FindProperty("heightPercent");
        base.OnEnable();
    }
    
    protected new void SetTransform()
    {
        FencePlacer qp = (FencePlacer)target;
        transform = qp.transform;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        try { GUILayout.Label("Previous Link: " + previousInstance.name); }
        catch { GUILayout.Label("Previous Link: null"); }
    }

    protected override void MouseDown(RaycastHit hit)
    {
        List<Transform> children = new List<Transform>();
        hit.transform.GetComponentsInChildren(children);
        GameObject fencePost = hit.collider.gameObject.name.Contains("[Post]") ? hit.collider.gameObject : null;

        if (fencePost)
        {
            if (previousInstance)
            {
                CreateMesh(previousInstance.transform, fencePost.transform, (Material)fenceMat.objectReferenceValue);
                instance = fencePost;
                previousInstance = null;
            }
            else
                previousInstance = fencePost;
        }
        else
        {
            base.MouseDown(hit);
            instance.name = "[Post]" + instance.name;
        }
    }

    protected override void Drag(RaycastHit hit)
    {
        if (snap.boolValue)
        {
            // Snap to previous pole's position.
            if (previousInstance)
            {
                Vector3 prevPos = previousInstance.transform.position;
                Vector3 snapAxisPos = new Vector3();
                float x = Mathf.Abs(hit.point.x - prevPos.x);
                float z = Mathf.Abs(hit.point.z - prevPos.z);

                if (x > z)
                {
                    snapAxisPos = new Vector3(
                        Mathf.Round(hit.point.x),
                        Mathf.Round(prevPos.y),
                        Mathf.Round(prevPos.z)
                        );
                }
                else
                {
                    snapAxisPos = new Vector3(
                        Mathf.Round(prevPos.x),
                        Mathf.Round(prevPos.y),
                        Mathf.Round(hit.point.z)
                        );
                }

                instance.transform.position = snapAxisPos;
            }
            else
                base.Drag(hit);

        }
        else
            base.Drag(hit);
    }

    protected override void MouseUp()
    {
        if (previousInstance && instance)
            CreateMesh(previousInstance.transform, instance.transform, (Material)fenceMat.objectReferenceValue);
        base.MouseUp();
    }

    private GameObject CreateMesh(Transform from, Transform to, Material material)
    {
        Mesh mesh = new Mesh();

        StretchMesh(from, to, mesh);

        GameObject go = new GameObject("[Fence]", typeof(MeshFilter), typeof(MeshRenderer));
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.GetComponent<MeshRenderer>().material = material;
        go.transform.SetParent(from.transform, false);

        return go;
    }

    private void StretchMesh(Transform from, Transform to, Mesh mesh)
    {
        // Get positions to draw mesh between.
        Vector3 startPole = Vector3.zero;
        Vector3 endPole = from.InverseTransformPoint(to.position);        // Is affected by scale.

        // Trys to get the prefab root. Most cases there will be a prefab for the poles, but this is incase there is simply a scene object.
        GameObject fromSource = PrefabUtility.GetCorrespondingObjectFromSource(from.gameObject) ?? from.gameObject;
        GameObject toSource = PrefabUtility.GetCorrespondingObjectFromSource(to.gameObject) ?? to.gameObject;

        // Make bounds including all children for both poles. Ensures the percent scaling is more accurate.
        // NOTE: Some light debugging my indicate that the foreach loops to include the children in the bounds my be unneccesary, but will keep for now.
        Bounds startPoleBounds = fromSource.GetComponentInChildren<MeshFilter>().sharedMesh.bounds;
        foreach (Transform child in fromSource.GetComponentsInChildren<Transform>())
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf)
                startPoleBounds.Encapsulate(mf.sharedMesh.bounds);
        }
        Bounds endPoleBounds = toSource.GetComponentInChildren<MeshFilter>().sharedMesh.bounds;
        foreach(Transform child in toSource.GetComponentsInChildren<Transform>())
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf)
                endPoleBounds.Encapsulate(mf.sharedMesh.bounds);
        }
        float prevPoleHeight = startPoleBounds.size.y * heightPercent.floatValue;
        float nextPoleHeight = endPoleBounds.size.y * heightPercent.floatValue;
        
        // Set verts
        // Note: Takes the first found mesh filter.
        mesh.vertices = new Vector3[]
        {
            startPole,
            endPole,
            new Vector3(startPole.x, prevPoleHeight, startPole.z),
            new Vector3(endPole.x, nextPoleHeight, endPole.z)

            /* 
             * 2------>3
             * ^__
             *    \__
             *       \
             * 0------>1
             */
        };
        mesh.RecalculateBounds();

        // Set UVs
        // Calculate the UV X value so it tiles properly.
        float uvScale = (mesh.bounds.size.x + mesh.bounds.size.z)/ mesh.bounds.size.y;
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(uvScale, 0),
            new Vector2(0, 1),
            new Vector2(uvScale, 1)
        };
        
        // Draw tris
        mesh.triangles = new int[]
        {
            0,2,1,
            2,3,1,
            // Backfacing
            1,2,0,
            1,3,2
        };

        // Normals
        mesh.normals = new Vector3[]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
    }
}
