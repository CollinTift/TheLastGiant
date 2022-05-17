using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noiseMap) {
        int xLength = noiseMap.GetLength(0);
        int zLength = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(xLength, zLength);

        Color[] colorMap = new Color[xLength * zLength];
        for (int z = 0; z < zLength; z++) {
            for (int x = 0; x < xLength; x++) {
                colorMap[z * xLength + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(xLength, 1, zLength);
    }
}
