using System;
using System.Threading;
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

    public Noise.NormalizeMode normalizeMode;

    public const int MAP_CHUNK_SIZE = 241;
    [Range(0, 6)]
    public int previewLOD;

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

    Queue<MapThreadInfo<MapData>> mapDataThreadInfo = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfo = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch(drawMode) {
            case DrawMode.NOISE_MAP:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.COLOR_MAP:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
                break;
            case DrawMode.MESH:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMult, meshHeightCurve, previewLOD), 
                                    TextureGenerator.TextureFromColorMap(mapData.colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
                break;
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback) {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfo) {
            mapDataThreadInfo.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMult, meshHeightCurve, lod);
        lock (meshDataThreadInfo) {
            meshDataThreadInfo.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update() {
        if (mapDataThreadInfo.Count > 0) {
            for (int i = 0; i < mapDataThreadInfo.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfo.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfo.Count > 0) {
            for (int i = 0; i < meshDataThreadInfo.Count; i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfo.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 center) {
        float[,] noiseMap = Noise.GenerateNoiseMap(MAP_CHUNK_SIZE, MAP_CHUNK_SIZE, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

        Color[] colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
        for (int y = 0; y < MAP_CHUNK_SIZE; y++) {
            for (int x = 0; x < MAP_CHUNK_SIZE; x++) {
                float currentMAP_CHUNK_SIZE = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++) {
                    if (currentMAP_CHUNK_SIZE >= regions[i].height) {
                        colorMap[y * MAP_CHUNK_SIZE + x] = regions[i].color;
                    } else {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate() {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}