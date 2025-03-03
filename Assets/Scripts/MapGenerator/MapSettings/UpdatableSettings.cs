﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableSettings : ScriptableObject
{
	public event System.Action OnValuesUpdated;
	public bool autoUpdate;

#if UNITY_EDITOR

	public void NotifyUpdatedValues()
	{
		UnityEditor.EditorApplication.update -= NotifyUpdatedValues;
		OnValuesUpdated?.Invoke();
	}

	protected virtual void OnValidate()
	{
		if (autoUpdate)
		{
			UnityEditor.EditorApplication.update += NotifyUpdatedValues;
		}
	}

#endif
}
