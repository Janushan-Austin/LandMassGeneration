using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public int LODIndex;

	public HeightMapSettings heightMapSettings;
	public MeshSettings meshSettings;
	public TextureSettings textureSettings;
    
    public Transform viewer;
	public Material terrainMaterial;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
		textureSettings.ApplyToMaterial(terrainMaterial);
		textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		meshWorldSize = meshSettings.meshWorldSize;
		float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);


		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / meshSettings.meshScale;
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
			if( i < 0 || i > visibleTerrainChunks.Count)
			{
				Debug.LogError("i is outside bounds of visbleTerrainChunks");
			}
			TerrainChunk chunk = visibleTerrainChunks[i];
			chunk.UpdateChunk();
			alreadyUpdatedChunks.Add(chunk.coord);
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
						TerrainChunk chunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, LODIndex, viewer, transform, terrainMaterial);
						terrainChunkDictionary.Add(viewedChunkCoord, chunk);
						chunk.OnVisibilityChanged += OnTerrainChunkVisibilityChanged;
						chunk.Load();

					}
					else
					{
						terrainChunkDictionary[viewedChunkCoord].UpdateChunk();
					}
				}
            }
        }
    }

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
	{
		if(isVisible == true)
		{
			visibleTerrainChunks.Add(chunk);
		}
		else
		{
			visibleTerrainChunks.Remove(chunk);
		}
	}
}


