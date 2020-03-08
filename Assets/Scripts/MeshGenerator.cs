using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
	public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings,  int levelOfDetail)
	{

		int skipIncrement = levelOfDetail * 2;
		if (skipIncrement == 0)
		{
			skipIncrement = 1;
		}

		int numVertsPerLine = meshSettings.numVerticesPerLine;

		Vector2 topLeft = new Vector2(-1,1) * meshSettings.meshWorldSize/2f;

		MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);
		int vertexIndex = 0;

		int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
		int meshVertexIndex = 0;
		int outOfMeshVertexIndex = -1;

		for (int y = 0; y < numVertsPerLine; y ++)
		{
			for (int x = 0; x < numVertsPerLine; x ++, vertexIndex++)
			{
				bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
				bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y>2 && y<numVertsPerLine-3 && ((x-2)%skipIncrement != 0 || (y-2)% skipIncrement !=0);
				if (isOutOfMeshVertex == true)
				{
					vertexIndicesMap[x, y] = outOfMeshVertexIndex;
					outOfMeshVertexIndex--;
				}
				else if(isSkippedVertex == false)
				{
					vertexIndicesMap[x, y] = meshVertexIndex;
					meshVertexIndex++;
				}
			}
		}


		for (int y = 0; y < numVertsPerLine; y ++)
		{
			for (int x = 0; x < numVertsPerLine; x ++, vertexIndex++)
			{
				bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

				if(isSkippedVertex == true) { continue; }

				bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
				bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
				bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
				bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

				vertexIndex = vertexIndicesMap[x, y];

				Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
				float height = heightMap[x, y];
				Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;


				if (isEdgeConnectionVertex)
				{
					bool isVertical = x == 2 || x == numVertsPerLine-3;

					int dstToMainVertexA = (isVertical ? y - 2 : x - 2) % skipIncrement;
					int dstToMainVertexB = skipIncrement - dstToMainVertexA;
					float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

					float heightMainVertexA = heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
					float heightMainVertexB = heightMap[(isVertical) ? x : x - dstToMainVertexB, (isVertical) ? y - dstToMainVertexB : y];

					height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
				}

				meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

				bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

				if (createTriangle)
				{
					int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

					int a = vertexIndicesMap[x, y];
					int b = vertexIndicesMap[x + currentIncrement, y];
					int c = vertexIndicesMap[x, y + currentIncrement];
					int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];


					meshData.AddTriangle(a, d, c);
					meshData.AddTriangle(d, a, b);
				}
			}
		}

		meshData.FinalizeMesh();

		return meshData;
	}
}


public class MeshData
{
	Vector3[] vertices;
	int[] triangles;
	Vector2[] uvs;

	Vector3[] outOfMeshVertices;
	int[] outOfMeshTriangles;

	int triangleIndex;
	int outOfMeshTriangleIndex;

	Vector3[] bakedNormals;

	bool useFlatShading;

	public MeshData(int verticesPerLine, int skipIncrement,  bool useFlatShading = false)
	{
		int numMeshEdgeVertices = (verticesPerLine - 2) * 4 - 4;
		int numEdgeConnectionVertices = (skipIncrement - 1) * (verticesPerLine - 5) / skipIncrement * 4;
		int numMainVerticesPerLine = (verticesPerLine - 5) / skipIncrement + 1;
		int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

		vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
		uvs = new Vector2[vertices.Length];

		int numMeshEdgeTriangles = 8*(verticesPerLine - 4);
		int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;

		triangles = new int[(numMeshEdgeTriangles + numMainTriangles)*3];
		triangleIndex = 0;

		outOfMeshVertices = new Vector3[verticesPerLine * 4 - 4];
		outOfMeshTriangles = new int[(verticesPerLine -2) * 24];
		outOfMeshTriangleIndex = 0;

		this.useFlatShading = useFlatShading;
	}

	public void AddVertex(Vector3 vertexPosition, Vector2 uv, int index)
	{
		if (index < 0)
		{
			outOfMeshVertices[-index - 1] = vertexPosition;
		}
		else
		{
			vertices[index] = vertexPosition;
			uvs[index] = uv;
		}
	}

	public void AddTriangle(int a, int b, int c)
	{
		if (a < 0 || b < 0 || c < 0)
		{
			outOfMeshTriangles[outOfMeshTriangleIndex] = a;
			outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
			outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
			outOfMeshTriangleIndex += 3;
		}
		else
		{
			triangles[triangleIndex] = a;
			triangles[triangleIndex + 1] = b;
			triangles[triangleIndex + 2] = c;
			triangleIndex += 3;
		}

	}

	Vector3[] CalculateNormals()
	{
		Vector3[] vertexNormals = new Vector3[vertices.Length];
		int triangleCount = triangles.Length / 3;

		for (int i = 0; i < triangleCount; i++)
		{
			int normalTriangleIndex = i * 3;
			int indexA = triangles[normalTriangleIndex];
			int indexB = triangles[normalTriangleIndex + 1];
			int indexC = triangles[normalTriangleIndex + 2];

			Vector3 triangleNormal = CalculateNormal(indexA, indexB, indexC);
			vertexNormals[indexA] += triangleNormal;
			vertexNormals[indexB] += triangleNormal;
			vertexNormals[indexC] += triangleNormal;
		}

		int borderTriangleCount = outOfMeshTriangles.Length / 3;
		for (int i = 0; i < borderTriangleCount; i++)
		{
			int normalTriangleIndex = i * 3;
			int indexA = outOfMeshTriangles[normalTriangleIndex];
			int indexB = outOfMeshTriangles[normalTriangleIndex + 1];
			int indexC = outOfMeshTriangles[normalTriangleIndex + 2];

			Vector3 triangleNormal = CalculateNormal(indexA, indexB, indexC);
			if (indexA >= 0)
			{
				vertexNormals[indexA] += triangleNormal;
			}
			if (indexB >= 0)
			{
				vertexNormals[indexB] += triangleNormal;
			}
			if (indexC >= 0)
			{
				vertexNormals[indexC] += triangleNormal;
			}
		}

		for (int i = 0; i < vertexNormals.Length; i++)
		{
			vertexNormals[i].Normalize();
		}

		return vertexNormals;
	}

	Vector3 CalculateNormal(int indexA, int indexB, int indexC)
	{
		Vector3 pointA = indexA >= 0 ? vertices[indexA] : outOfMeshVertices[-indexA - 1];
		Vector3 pointB = indexB >= 0 ? vertices[indexB] : outOfMeshVertices[-indexB - 1];
		Vector3 pointC = indexC >= 0 ? vertices[indexC] : outOfMeshVertices[-indexC - 1];

		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;

		return Vector3.Cross(sideAB, sideAC).normalized;
	}

	public void FinalizeMesh()
	{
		if (useFlatShading)
		{
			FlatShading();
		}
		else
		{
			BakeNormals();
		}
	}

	void BakeNormals()
	{
		bakedNormals = CalculateNormals();
	}

	void FlatShading()
	{
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUVs = new Vector2[triangles.Length];

		for(int i = 0; i < triangles.Length; i++)
		{
			flatShadedVertices[i] = vertices[triangles[i]];
			flatShadedUVs[i] = uvs[triangles[i]];
			triangles[i] = i;
		}

		vertices = flatShadedVertices;
		uvs = flatShadedUVs;
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		if (useFlatShading)
		{
			mesh.RecalculateNormals();
		}
		else
		{
			mesh.normals = bakedNormals;
		}
		//mesh.RecalculateNormals();
		return mesh;
	}
}