using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdatableData
{
	public bool useFlatShading;
	public bool useFallOff;
	public float meshHeightMultiplier;
	public float uniformScale = 1;
	public AnimationCurve meshHeightCurve;

	public float minHeight { get { return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0); } }
	public float maxHeight { get { return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1); } }
}
