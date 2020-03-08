using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableSettings
{
	public float meshScale = 1;
	public bool useFlatShading;

	public const int numSupportedLODs = 5;
	public const int numSupportedChunkSizes = 9;
	public const int numSupportedFlatShadedChunkSizes = 3;
	public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

	[Range(0, numSupportedChunkSizes - 1)]
	public int chunkSizeIndex;

	[Range(0, numSupportedFlatShadedChunkSizes - 1)]
	public int flatShadedChunkSizeIndex;


	//num of vertices per line of a mesh rendered at heightest LOD (LOD == 0). Includes the 2 extra vertices that are excluded from final mesh but used for calculating normals
	public int numVerticesPerLine
	{
		get
		{
			return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;
		}
	}

	public float meshWorldSize
	{
		get
		{
			return (numVerticesPerLine - 3) * meshScale;
		}
	}

}
