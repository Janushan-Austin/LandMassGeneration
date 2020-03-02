using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode { NOISE, COLORMAP, MESH}
	public DrawMode drawMode;

	public const int mapChunkSize = 241;
	[Range(0,6)]
	public int levelOfDetail;

    //public int mapChunkSize =10, mapChunkSize = 10;
    public float noiseScale =0.3f;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

    public bool autoUpdate = true;

	public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
		for(int y= 0; y < mapChunkSize; y++)
		{
			for(int x = 0; x < mapChunkSize; x++)
			{
				float currentHeight = noiseMap[x, y];
				for(int r= 0; r< regions.Length; r++)
				{
					if(currentHeight <= regions[r].height)
					{
						colorMap[y * mapChunkSize + x] = regions[r].color;
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
				mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
				break;

			case DrawMode.MESH:
				mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
				break;
		}
    }

	private void OnValidate()
	{
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
