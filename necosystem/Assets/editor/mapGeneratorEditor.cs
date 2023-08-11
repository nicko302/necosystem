using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(mapGenerator))]
public class mapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        mapGenerator mapGen = (mapGenerator)target;

        if (DrawDefaultInspector())
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor(); //if the map gen variables have been altered, generate map
            }

        if (GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor(); //if Generate button is pressed, generate map
        }
    }
}
