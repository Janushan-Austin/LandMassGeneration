using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endlessterrain : MonoBehaviour
{
	const float scale = 1;

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public static float maxViewDistance;
    
    public Transform viewer;
	public Material mapMaterial;

    public static Vector2 viewerPosition;
	static Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;

	int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
		mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);


		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
		UpdateVisibleChunks();
    }

	private void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate)
		{
			chunk.SetVisible(false);
		}
		terrainChunksVisibleLastUpdate.Clear();

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
					terrainChunkDictionary[viewedChunkCoord].UpdateChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		MeshCollider meshCollider;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;
		int LODColliderIndex;

		int previousLODIndex = -1;

		MapData mapData;
		bool mapDataReceived;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent = null, Material material = null)
        {
            position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            bounds = new Bounds(position, Vector2.one * size);

			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
            meshObject.transform.position = positionV3 * scale;
			meshObject.transform.localScale = Vector3.one * scale;
			meshObject.transform.parent = parent;

			if (material != null)
			{
				meshRenderer.material = material;
			}

			this.detailLevels = detailLevels;
			lodMeshes = new LODMesh[detailLevels.Length];
			LODColliderIndex = 0;
			for(int i=0; i < detailLevels.Length; i++)
			{
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
				if(detailLevels[i].useForCollider == true)
				{
					LODColliderIndex = i;
				}
			}

			mapDataReceived = false;

			mapGenerator.RequestMapData(position,OnMapDataReceived);
        }

		void OnMapDataReceived(MapData mapData)
		{
			this.mapData = mapData;
			mapDataReceived = true;

			Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateChunk();
		}

        public void UpdateChunk()
        {
			if (!mapDataReceived)
			{
				return;
			}
            float viewDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			bool visible = viewDistanceFromNearestEdge <= maxViewDistance;

			if (visible)
			{
				int lodIndex = 0;
				for(; lodIndex < detailLevels.Length-1; lodIndex++)
				{
					if(detailLevels[lodIndex].visibleDistanceThreshold >= viewDistanceFromNearestEdge)
					{
						break;
					}
				}
				if(lodIndex != previousLODIndex)
				{
					LODMesh lodMesh = lodMeshes[lodIndex];
					if (lodMesh.hasMesh)
					{
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					}
					else if(!lodMesh.hasRequestedMesh)
					{
						lodMesh.RequestMesh(mapData);
					}
				}

				if(lodIndex == 0)
				{
					if (lodMeshes[LODColliderIndex].hasMesh)
					{
						meshCollider.sharedMesh = lodMeshes[LODColliderIndex].mesh;
					}
					else if (!lodMeshes[LODColliderIndex].hasRequestedMesh)
					{
						lodMeshes[LODColliderIndex].RequestMesh(mapData);
					}
				}
				terrainChunksVisibleLastUpdate.Add(this);
			}

			SetVisible(visible);
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

		System.Action updateCallback;

		public LODMesh(int lod, System.Action callBack)
		{
			this.lod = lod;
			updateCallback = callBack;
			hasRequestedMesh = hasMesh = false;
		}

		void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;
			updateCallback();
		}

		public void RequestMesh(MapData mapData)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
		}
	}

	[System.Serializable]
	public struct LODInfo
	{
		public int lod;
		public float visibleDistanceThreshold;
		public bool useForCollider;
	}
}


