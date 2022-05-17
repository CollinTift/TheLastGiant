using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public int xLength;
    public int zLength;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(xLength, zLength, seed, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    private void OnValidate() {
        if (xLength < 1) xLength = 1;
        if (zLength < 1) zLength = 1;

        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}
