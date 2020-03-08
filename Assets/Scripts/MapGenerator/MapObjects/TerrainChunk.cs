using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
	const float colliderGenerationDistanceThreshold = 5f;

	GameObject meshObject;
	Vector2 sampleCenter;
	Bounds bounds;
	public Vector2 coord;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	MeshCollider meshCollider;

	LODInfo[] detailLevels;
	LODMesh[] lodMeshes;
	int LODColliderIndex;
	bool hasSetCollider;

	float maxViewDistance;

	int previousLODIndex = -1;

	HeightMapSettings heightMapSettings;
	MeshSettings meshSettings;
	HeightMap heightMap;
	bool heightMapReceived;
	bool heightMapRequested;

	Transform viewer;
	Vector2 viewerPosition { get { return new Vector2(viewer.position.x, viewer.position.z); } }

	public event System.Action<TerrainChunk, bool> OnVisibilityChanged;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int lodIndex, Transform viewer, Transform parent = null, Material material = null, System.Action<TerrainChunk, bool> visibiltyCallback = null)
	{
		this.coord = coord;
		this.meshSettings = meshSettings;
		this.heightMapSettings = heightMapSettings;
		this.viewer = viewer;
		this.detailLevels = detailLevels;
		heightMapReceived = false;
		heightMapRequested = false;

		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;

		sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize;

		bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();

		meshObject.transform.position = new Vector3(position.x, 0, position.y);
		meshObject.transform.parent = parent;

		if (material != null)
		{
			meshRenderer.material = material;
		}

		LODColliderIndex = lodIndex;
		hasSetCollider = false;

		lodMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++)
		{
			lodMeshes[i] = new LODMesh(detailLevels[i].lod);
			lodMeshes[i].updateCallback += UpdateChunk;
			if (i == LODColliderIndex)
			{
				lodMeshes[i].updateCallback += UpdateCollisionMesh;
			}
		}

		SetVisible(false);


		if (visibiltyCallback != null)
		{
			OnVisibilityChanged += visibiltyCallback;
			heightMapRequested = true;
			ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
		}
	}

	public void Load()
	{
		if(heightMapRequested == false)
		{
			heightMapRequested = true;
			ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
		}
	}

	void OnHeightMapReceived(object heightMap)
	{
		this.heightMap = (HeightMap)heightMap;
		heightMapReceived = true;

		UpdateChunk();
	}

	public void UpdateChunk()
	{
		if (!heightMapReceived)
		{
			return;
		}
		float viewDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
		bool wasVisible = IsVisible();
		bool visible = viewDistanceFromNearestEdge <= maxViewDistance;

		if (visible)
		{
			int terrainLODIndex = 0;
			for (; terrainLODIndex < detailLevels.Length - 1; terrainLODIndex++)
			{
				if (detailLevels[terrainLODIndex].visibleDistanceThreshold >= viewDistanceFromNearestEdge)
				{
					break;
				}
			}
			if (terrainLODIndex != previousLODIndex)
			{
				LODMesh lodMesh = lodMeshes[terrainLODIndex];
				if (lodMesh.hasMesh)
				{
					previousLODIndex = terrainLODIndex;
					meshFilter.mesh = lodMesh.mesh;
				}
				else if (!lodMesh.hasRequestedMesh)
				{
					lodMesh.RequestMesh(heightMap, meshSettings);
				}
			}
		}
		if (wasVisible != visible)
		{
			SetVisible(visible);
			OnVisibilityChanged?.Invoke(this, visible);
		}
	}

	public void UpdateCollisionMesh()
	{
		if (hasSetCollider == false)
		{
			float sqrDst = bounds.SqrDistance(viewerPosition);

			if (sqrDst < detailLevels[LODColliderIndex].sqrVisibleDstTresh)
			{
				if (lodMeshes[LODColliderIndex].hasRequestedMesh == false)
				{
					lodMeshes[LODColliderIndex].RequestMesh(heightMap, meshSettings);
				}
			}

			if (sqrDst < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
			{
				if (lodMeshes[LODColliderIndex].hasMesh)
				{
					meshCollider.sharedMesh = lodMeshes[LODColliderIndex].mesh;
					hasSetCollider = true;
				}
			}
		}
	}

	public void SetVisible(bool visible)
	{
		meshObject.SetActive(visible);
	}

	public bool IsVisible()
	{
		return meshObject.activeSelf;
	}
}
