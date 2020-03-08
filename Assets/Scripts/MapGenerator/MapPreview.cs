using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
	public enum DrawMode { NOISE, COLORMAP, MESH, FALLOFF }
	public DrawMode drawMode;

	//float[,] fallOffMap;

	public Renderer textureRenderer;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureSettings textureData;

	public Material terrainMaterial;

	[Range(0, MeshSettings.numSupportedLODs - 1)]
	public int editorPreviewLevelOfDetail;

	public bool autoUpdate = true;

	public void DrawMapInEditor()
	{
		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, Vector2.zero);
		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeights(terrainMaterial, heightMap.minValue, heightMap.maxValue);
		switch (drawMode)
		{
			case DrawMode.NOISE:
				DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
				break;

			case DrawMode.MESH:
				DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLevelOfDetail));
				break;

			case DrawMode.FALLOFF:
				DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVerticesPerLine), 0, 1)));
				break;
		}
	}

	public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) /10f;

		textureRenderer.gameObject.SetActive(true);
		meshFilter.gameObject.SetActive(false);
    }

	public void DrawMesh(MeshData meshData)
	{
		meshFilter.sharedMesh = meshData.CreateMesh();

		textureRenderer.gameObject.SetActive(false);
		meshFilter.gameObject.SetActive(true);
	}

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

	private void OnValidate()
	{
		if (meshSettings != null)
		{
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if (heightMapSettings != null)
		{
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if (textureData != null)
		{
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}

	}
}
