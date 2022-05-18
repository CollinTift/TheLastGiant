using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {
        NOISE_MAP,
        COLOR_MAP,
        MESH
    }

    public DrawMode drawMode;

    public int width;
    public int height;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMult;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colorMap[y * width + x] = regions[i].color;
                        break;
                    } 
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch(drawMode) {
            case DrawMode.NOISE_MAP:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.COLOR_MAP:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, width, height));
                break;
            case DrawMode.MESH:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMult, meshHeightCurve), TextureGenerator.TextureFromColorMap(colorMap, width, height));
                break;
        }
    }

    private void OnValidate() {
        if (width < 1) width = 1;
        if (height < 1) height = 1;

        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}