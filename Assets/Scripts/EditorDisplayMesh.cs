using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DisplayMesh))]
public class EditorDisplayMesh : Editor {

    public Material m_material;

    public override void OnInspectorGUI()
    {
        //MapGenerator mapGen = (MapGenerator)target;
        DisplayMesh display = (DisplayMesh)target;

        if (DrawDefaultInspector())
        {
            if (display.autoUpdate)
            {
                display.MakeMesh();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            display.MakeMesh();
        }
    }
}
