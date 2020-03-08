using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableSettings
{
	public NoiseSettings noiseSettings;

	public bool useFallOff;
	public float HeightMultiplier;

	public AnimationCurve HeightCurve = new AnimationCurve();

	public float minHeight { get { return HeightMultiplier * HeightCurve.Evaluate(0); } }
	public float maxHeight { get { return HeightMultiplier * HeightCurve.Evaluate(1); } }


#if UNITY_EDITOR
	protected override void OnValidate()
	{
		noiseSettings.ValidateValues();

		base.OnValidate();
	}

#endif
}
