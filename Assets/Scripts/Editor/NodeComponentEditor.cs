using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(NodeComponent))]
public class NodeComponentEditor : Editor
{
    private void OnSceneGUI()
    {
        NodeComponent nodeComponent = (NodeComponent)target;
        Handles.color = Color.white;
        EditorGUI.BeginChangeCheck();
        float newRadius = Handles.RadiusHandle(Quaternion.identity, nodeComponent.transform.position, nodeComponent.radius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(nodeComponent, "Change Circle Radius");
            nodeComponent.radius = newRadius;
        }
    }
}
