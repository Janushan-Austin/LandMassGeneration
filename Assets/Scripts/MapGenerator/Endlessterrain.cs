using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endlessterrain : MonoBehaviour
{
	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
	const float colliderGenerationDistanceThreshold = 5f;

	public LODInfo[] detailLevels;
	public int LODIndex;
	public static float maxViewDistance;
    
    public Transform viewer;
	public Material mapMaterial;

    public static Vector2 viewerPosition;
	static Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;

	float meshWorldSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
		mapGenerator = FindObjectOfType<MapGenerator>();
        meshWorldSize = mapGenerator.meshSettings.meshWorldSize;
		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);


		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.meshSettings.meshScale;
		UpdateVisibleChunks();
    }

	private void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

		if(viewerPosition != viewerPositionOld)
		{
			foreach(TerrainChunk chunk in visibleTerrainChunks)
			{
				chunk.UpdateCollisionMesh();
			}
		}

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

		HashSet<Vector2> alreadyUpdatedChunks = new HashSet<Vector2>();
		for(int i = visibleTerrainChunks.Count - 1; i>= 0; i--)
		{
			visibleTerrainChunks[i].UpdateChunk();
			alreadyUpdatedChunks.Add(visibleTerrainChunks[i].coord);
		}

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunks.Contains(viewedChunkCoord))
				{
					if (!terrainChunkDictionary.ContainsKey(viewedChunkCoord))
					{
						terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, meshWorldSize, detailLevels, LODIndex, transform, mapMaterial));
					}
					else
					{
						terrainChunkDictionary[viewedChunkCoord].UpdateChunk();
					}
				}
            }
        }
    }

	public class TerrainChunk
	{
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

		int previousLODIndex = -1;

		HeightMap heightMap;
		bool heightMapReceived;

		public TerrainChunk(Vector2 coord, float meshWorldSize, LODInfo[] detailLevels, int lodIndex, Transform parent = null, Material material = null)
		{
			this.coord = coord;
			sampleCenter = coord * meshWorldSize / mapGenerator.meshSettings.meshScale;
			Vector2 position = coord * meshWorldSize;

			bounds = new Bounds(position, Vector2.one * meshWorldSize);

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

			this.detailLevels = detailLevels;
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

			heightMapReceived = false;

			mapGenerator.RequestHeightMap(sampleCenter, OnHeightMapReceived);
		}

		void OnHeightMapReceived(HeightMap heightMap)
		{
			this.heightMap = heightMap;
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
						lodMesh.RequestMesh(heightMap);
					}
				}				
			}
			if (wasVisible != visible)
			{
				if(visible == true)
				{
					visibleTerrainChunks.Add(this);
				}
				else
				{
					visibleTerrainChunks.Remove(this);
				}
				SetVisible(visible);
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
						lodMeshes[LODColliderIndex].RequestMesh(heightMap);
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

	public class LODMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;

		public event System.Action updateCallback;

		public LODMesh(int lod)
		{
			this.lod = lod;
			hasRequestedMesh = hasMesh = false;
		}

		void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;
			updateCallback();
		}

		public void RequestMesh(HeightMap heightMap)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(heightMap, lod, OnMeshDataReceived);
		}
	}

	[System.Serializable]
	public struct LODInfo
	{
		[Range(0,MeshSettings.numSupportedLODs-1)]
		public int lod;
		public float visibleDistanceThreshold;

		public float sqrVisibleDstTresh { get { return visibleDistanceThreshold * visibleDistanceThreshold; } }
	}
}


