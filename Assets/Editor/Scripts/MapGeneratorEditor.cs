using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MapGenerator map = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (map.autoUpdate == true)
            {
                map.GenerateMap();
            }
        }

        if (CustomEditorGUI.Buttons.AddButton("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
