using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableSettings), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		UpdatableSettings data = (UpdatableSettings)target;

		if(CustomEditorGUI.Buttons.AddButton("Update data"))
		{
			data.NotifyUpdatedValues();
			EditorUtility.SetDirty(target);
		}
	}
}
