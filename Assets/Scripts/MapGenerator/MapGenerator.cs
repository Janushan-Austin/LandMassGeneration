using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode { NOISE, COLORMAP}
	public DrawMode drawMode;

    public int mapWidth =10, mapHeight = 10;
    public float noiseScale =0.3f;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

    public bool autoUpdate = true;

	public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colorMap = new Color[mapHeight * mapWidth];
		for(int y= 0; y < mapHeight; y++)
		{
			for(int x = 0; x < mapWidth; x++)
			{
				float currentHeight = noiseMap[x, y];
				for(int r= 0; r< regions.Length; r++)
				{
					if(currentHeight <= regions[r].height)
					{
						colorMap[y * mapWidth + x] = regions[r].color;
						break;
					}
				}
			}
		}

		MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
		switch (drawMode)
		{
			case DrawMode.NOISE:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
				break;
			case DrawMode.COLORMAP:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
				break;
		}
    }

	private void OnValidate()
	{
		if(mapWidth < 1)
		{
			mapWidth = 1;
		}
		if(mapHeight < 1)
		{
			mapHeight = 1;
		}
		if(octaves < 1)
		{
			octaves = 1;
		}
		if(lacunarity < 1)
		{
			lacunarity = 1;
		}
	}

	[System.Serializable]
	public struct TerrainType
	{
		public string name;
		public float height;
		public Color color;
	}
}
