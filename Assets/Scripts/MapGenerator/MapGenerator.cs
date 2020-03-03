using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode { NOISE, COLORMAP, MESH}
	public DrawMode drawMode;

	public const int mapChunkSize = 241;
	[Range(0,6)]
	public int editorPreviewLevelOfDetail;

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

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	private void Update()
	{
		if(mapDataThreadInfoQueue.Count > 0)
		{
			for(int i=0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callBack(threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callBack(threadInfo.parameter);
			}
		}
	}

	public void DrawMapInEditor()
	{
		MapData mapData = GenerateMapData(Vector2.zero);
		MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
		switch (drawMode)
		{
			case DrawMode.NOISE:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
				break;
			case DrawMode.COLORMAP:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
				break;

			case DrawMode.MESH:
				mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLevelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
				break;
		}
	}

	public void RequestMapData(Vector2 center, System.Action<MapData> callBack)
	{
		ThreadStart threadStart = delegate { MapDataThread(center, callBack); };

		new Thread(threadStart).Start();
	}

	void MapDataThread(Vector2 center, System.Action<MapData> callBack)
	{
		MapData mapData = GenerateMapData(center);
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));
		}
	}

	public void RequestMeshData(MapData mapData, int lod, System.Action<MeshData> callBack)
	{
		ThreadStart threadStart = delegate { MeshDataThread(mapData ,lod, callBack); };

		new Thread(threadStart).Start();
	}

	void MeshDataThread(MapData mapData, int lod, System.Action<MeshData> callBack)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
		}
	}

	private MapData GenerateMapData(Vector2 center)
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset);

		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++)
		{
			for (int x = 0; x < mapChunkSize; x++)
			{
				float currentHeight = noiseMap[x, y];
				for (int r = 0; r < regions.Length; r++)
				{
					if (currentHeight <= regions[r].height)
					{
						colorMap[y * mapChunkSize + x] = regions[r].color;
						break;
					}
				}
			}
		}

		return new MapData(noiseMap, colorMap);
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

	struct MapThreadInfo<T>
	{
		public readonly System.Action<T> callBack;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callBack, T parameter)
		{
			this.callBack = callBack;
			this.parameter = parameter;
		}
	}
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}


public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;

	public MapData(float[,] heightMap, Color[] colorMap)
	{
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}
}
