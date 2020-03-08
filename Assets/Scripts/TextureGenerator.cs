using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
	public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
	{
		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}


	public static Texture2D TextureFromHeightMap(HeightMap heightMap)
	{
		int width = 0;
		int height = 0;
		if (heightMap.values != null)
		{
			width = heightMap.values.GetLength(0);
			height = heightMap.values.GetLength(1);
		}

		Color[] colorMap = new Color[width * height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, Utils.Math.Map(heightMap.values[x,y], heightMap.minValue, heightMap.maxValue, 0, 1));
			}
		}

		return TextureFromColorMap(colorMap, width, height);
	}
}
