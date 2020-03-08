using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapPreview))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        MapPreview map = (MapPreview)target;

        if (DrawDefaultInspector())
        {
            if (map.autoUpdate == true)
            {
                map.DrawMapInEditor();
            }
        }

        if (CustomEditorGUI.Buttons.AddButton("Generate Map"))
        {
            map.DrawMapInEditor();
        }
    }
}
