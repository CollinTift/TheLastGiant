using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    public const float MAX_VIEW_DIST = 450;

    public Transform viewer;
    public static Vector2 viewerPos;

    int chunkSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();

    private void Start() {
        chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        chunksVisible = Mathf.RoundToInt(MAX_VIEW_DIST / chunkSize);
    }

    private void Update() {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {
        for (int i = 0; i < lastVisibleChunks.Count; i++) {
            lastVisibleChunks[i].SetVisible(false);
        }
        lastVisibleChunks.Clear();

        Vector2 currChunkCoord = new Vector2(Mathf.RoundToInt(viewerPos.x / chunkSize), Mathf.RoundToInt(viewerPos.y / chunkSize));

        for (int yOffset = -chunksVisible; yOffset <= chunksVisible; yOffset++) {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++) {
                Vector2 viewChunkCoord = new Vector2(currChunkCoord.x + xOffset, currChunkCoord.y + yOffset);

                if (chunkDictionary.ContainsKey(viewChunkCoord)) {
                    chunkDictionary[viewChunkCoord].UpdateTerrainChunk();
                    if (chunkDictionary[viewChunkCoord].IsVisible()) lastVisibleChunks.Add(chunkDictionary[viewChunkCoord]);
                } else {
                    chunkDictionary.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObj;
        Vector2 pos;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent) {
            pos = coord * size;
            bounds = new Bounds(pos, Vector2.one * size);
            Vector3 pos3D = new Vector3(pos.x, 0, pos.y);

            meshObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObj.transform.position = pos3D;
            meshObj.transform.localScale = Vector3.one * size / 10f;
            meshObj.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk() {
            float distFromNearEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
            bool visible = distFromNearEdge <= MAX_VIEW_DIST;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObj.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObj.activeSelf;
        }
    }
}
