using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
	public event System.Action OnValuesUpdated;
	public bool autoUpdate;

	public void NotifyUpdatedValues()
	{
		OnValuesUpdated?.Invoke();
	}

	protected virtual void OnValidate()
	{
		if (autoUpdate)
		{
			NotifyUpdatedValues();
		}
	}
}
