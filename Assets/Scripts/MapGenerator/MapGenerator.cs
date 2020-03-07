using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{

	public enum DrawMode { NOISE, COLORMAP, MESH, FALLOFF}
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureData;

	public Material terrainMaterial;

	//public const int mapChunkSize = 95;
	
	[Range(0,MeshSettings.numSupportedLODs -1)]
	public int editorPreviewLevelOfDetail;
	
	public bool autoUpdate = true;

	Queue<MapThreadInfo<HeightMap>> heightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
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
		textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
	}

	private void Start()
	{
		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
	}


	private void Update()
	{
		if(heightMapThreadInfoQueue.Count > 0)
		{
			for(int i=0; i < heightMapThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<HeightMap> threadInfo = heightMapThreadInfoQueue.Dequeue();
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
		textureData.ApplyToMaterial(terrainMaterial);
		//textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, Vector2.zero);
		MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
		switch (drawMode)
		{
			case DrawMode.NOISE:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
				break;

			case DrawMode.MESH:
				mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLevelOfDetail));
				break;

			case DrawMode.FALLOFF:
				mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVerticesPerLine)));
				break;
		}
	}

	public void RequestHeightMap(Vector2 center, System.Action<HeightMap> callBack)
	{
		ThreadStart threadStart = delegate { HeightMapThread(center, callBack); };

		new Thread(threadStart).Start();
	}

	void HeightMapThread(Vector2 center, System.Action<HeightMap> callBack)
	{
		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, center);
		lock (heightMapThreadInfoQueue)
		{
			heightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callBack, heightMap));
		}
	}

	public void RequestMeshData(HeightMap heightMap, int lod, System.Action<MeshData> callBack)
	{
		ThreadStart threadStart = delegate { MeshDataThread(heightMap ,lod, callBack); };

		new Thread(threadStart).Start();
	}

	void MeshDataThread(HeightMap heightMap, int lod, System.Action<MeshData> callBack)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
		}
	}

	private void OnValidate()
	{ 
		if(meshSettings != null)
		{
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if(heightMapSettings != null)
		{
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
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


