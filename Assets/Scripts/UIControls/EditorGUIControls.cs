using UnityEngine;
using UnityEditor;
//using EditorGUITable;

namespace CustomEditorGUI
{
	public class Layout
	{
		public static bool AddFoldout(bool foldoutProperty, string foldoutName)
		{
			return EditorGUILayout.Foldout(foldoutProperty, foldoutName);
		}

		public static void BeginHorizontal()
		{
			EditorGUILayout.BeginHorizontal();
		}

		public static void EndHorizontal()
		{
			EditorGUILayout.EndHorizontal();
		}

		public static void AddSpace(int pixels)
		{
			GUILayout.Space(pixels);
		}

		public static void FlexibleSpace()
		{
			GUILayout.FlexibleSpace();
		}

		public static float GUIWindowWidth()
		{
			return EditorGUIUtility.currentViewWidth;
		}
	}

	public class Sliders
	{
		public static void AddSlider(SerializedProperty property, float minValue, float maxValue, GUIContent label, params GUILayoutOption[] options)
		{
			EditorGUILayout.Slider(property, minValue, maxValue, label, options);
		}

		public static void AddSlider(SerializedProperty property, float minValue, float maxValue, string label, params GUILayoutOption[] options)
		{
			EditorGUILayout.Slider(property, minValue, maxValue, label, options);
			
		}

		public static float AddSlider(string label, float value, float minValue, float maxValue, params GUILayoutOption[] options)
		{
			return EditorGUILayout.Slider(label, value, minValue, maxValue, options);
		}

		public static void AddIntSlider(SerializedProperty property, int minValue, int maxValue, GUIContent label, params GUILayoutOption[] options)
		{
			EditorGUILayout.IntSlider(property, minValue, maxValue, label, options);
		}

		public static void AddIntSlider(SerializedProperty property, int minValue, int maxValue, string label, params GUILayoutOption[] options)
		{
			EditorGUILayout.IntSlider(property, minValue, maxValue, label, options);
		}

		public static int AddIntSlider(string label, int value, int minValue, int maxValue, params GUILayoutOption[] options)
		{
			return EditorGUILayout.IntSlider(label, value, minValue, maxValue, options);
		}
	}

	public class Tables
	{
		//public static GUITableState DrawTable(GUITableState tableState, SerializedProperty property)
		//{
		//	return GUITableLayout.DrawTable(tableState, property);
		//}
	}

	public class Labels
	{
		public static void AddLabelField(string label, GUIStyle style, params GUILayoutOption[] options)
		{
			EditorGUILayout.LabelField(label, style, options);
		}

		public static void AddLabel(string label, GUIStyle style, params GUILayoutOption[] options)
		{
			GUILayout.Label(label, style, options);
		}

		public static void AddLabel(Texture texture, params GUILayoutOption[] options)
		{
			GUILayout.Label(texture, options);
		}
	}

	public class Properties
	{
		public static void AddPropertyField(SerializedProperty property, params GUILayoutOption[] options)
		{
			EditorGUILayout.PropertyField(property, options);
		}

		public static string AddTextField(string label, string text, params GUILayoutOption[] options)
		{
			return EditorGUILayout.TextField(label, text, options);
		}
	}

	public class Buttons
	{
		public static bool AddButton(string buttonText, params GUILayoutOption[] options)
		{
			return GUILayout.Button(buttonText, options);
		}
	}

	public class Toggles
	{
		public static bool AddToggle(string label, bool value, params GUILayoutOption[] options)
		{
			return EditorGUILayout.Toggle(label, value, options);
		}
	}

	public class ProgressBars
	{
		public static void DisplayProgressBar(string title, string info, float progress)
		{
			EditorUtility.DisplayProgressBar(title, info, progress);
		}

		public static void ClearProgressBar()
		{
			EditorUtility.ClearProgressBar();
		}
	}

}

