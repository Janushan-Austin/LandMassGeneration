using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public int mapWidth =10, mapHeight = 10;
    public float noiseScale =0.3f;

    public bool autoUpdate = true;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        mapDisplay.DrawNoiseMap(noiseMap);
    }
}
