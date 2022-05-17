using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public static float[,] GenerateNoiseMap(int xLength, int zLength, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[xLength, zLength];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetZ = prng.Next(-100000, 100000) + offset.y;
            
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        if (scale <= 0) scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = xLength / 2f;
        float halfHeight = zLength / 2f;

        for (int z = 0; z < zLength; z++) {
            for (int x = 0; x < xLength; x++) {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleZ = (z - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1; //returns [-1, 1]
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                noiseMap[x, z] = noiseHeight;
            }
        }

        for (int z = 0; z < zLength; z++) {
            for (int x = 0; x < xLength; x++) {
                noiseMap[x, z] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }
}
