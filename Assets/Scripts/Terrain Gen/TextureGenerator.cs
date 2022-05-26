using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(colorMap);
        tex.Apply();
        return tex;
    }

    public static Texture2D TextureFromHeightMap(float[,] noiseMap) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
}
