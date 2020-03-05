using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{

	public enum DrawMode { NOISE, COLORMAP, MESH, FALLOFF}
	public DrawMode drawMode;

	public TerrainData terrainData;
	public NoiseData noiseData;
	public TextureData textureData;

	public Material terrainMaterial;

	//public const int mapChunkSize = 95;
	public int mapChunkSize
	{
		get
		{
			if (terrainData.useFlatShading)
			{
				return 95;
			}
			else
			{
				return 239;
			}
		}
	}
	[Range(0,6)]
	public int editorPreviewLevelOfDetail;
	
	public bool autoUpdate = true;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	float[,] fallOffMap;

	void OnValuesUpdated()
	{
		if (!Application.isPlaying)
		{
			DrawMapInEditor();
		}
	}

	void OnTextureValuesUpdated()
	{
		textureData.ApplyToMaterial(terrainMaterial);
	}


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

			case DrawMode.MESH:
				mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLevelOfDetail, terrainData.useFlatShading));
				break;

			case DrawMode.FALLOFF:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(fallOffMap));
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
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
		}
	}

	private MapData GenerateMapData(Vector2 center)
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

		if (terrainData.useFallOff)
		{
			if (fallOffMap == null)
			{
				fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
			}


			for (int y = 0; y < fallOffMap.GetLength(1); y++)
			{
				for (int x = 0; x < fallOffMap.GetLength(0); x++)
				{
						noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
				}
			}
		}

		return new MapData(noiseMap);
	}

	private void OnValidate()
	{ 
		if(terrainData != null)
		{
			terrainData.OnValuesUpdated -= OnValuesUpdated;
			terrainData.OnValuesUpdated += OnValuesUpdated;
		}

		if(noiseData != null)
		{
			noiseData.OnValuesUpdated -= OnValuesUpdated;
			noiseData.OnValuesUpdated += OnValuesUpdated;
		}

		if(textureData != null)
		{
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
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



public struct MapData
{
	public readonly float[,] heightMap;


	public MapData(float[,] heightMap)
	{
		this.heightMap = heightMap;
	}
}
