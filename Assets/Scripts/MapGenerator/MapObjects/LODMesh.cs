using UnityEngine;

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

	void OnMeshDataReceived(object meshData)
	{
		mesh = ((MeshData)meshData).CreateMesh();
		hasMesh = true;
		updateCallback();
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
	{
		hasRequestedMesh = true;
		//mapGenerator.RequestMeshData(heightMap, lod, OnMeshDataReceived);

		ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
	}
}
