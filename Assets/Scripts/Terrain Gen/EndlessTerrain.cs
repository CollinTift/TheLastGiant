using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    const float SCALE = 5f;

    const float MOVEMENT_FOR_CHUNK_UPDATE = 25f;
    const float SQR_MOVEMENT_FOR_CHUNK_UPDATE = MOVEMENT_FOR_CHUNK_UPDATE * MOVEMENT_FOR_CHUNK_UPDATE;

    public LODInfo[] detailLevels;
    public static float maxViewDist;

    public Transform viewer;
    public static Vector2 viewerPos;
    Vector2 viewerPosOld;

    public Material mapMaterial;

    static MapGenerator mapGenerator;

    int chunkSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();

    private void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDist;
        chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        chunksVisible = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z) / SCALE;

        if ((viewerPosOld - viewerPos).sqrMagnitude > SQR_MOVEMENT_FOR_CHUNK_UPDATE) {
            viewerPosOld = viewerPos;
            UpdateVisibleChunks();
        }
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
                } else {
                    chunkDictionary.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObj;
        Vector2 pos;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;
        int prevLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
            this.detailLevels = detailLevels;

            pos = coord * size;
            bounds = new Bounds(pos, Vector2.one * size);
            Vector3 pos3D = new Vector3(pos.x, 0, pos.y);

            meshObj = new GameObject("Terrain Chunk");

            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObj.transform.position = pos3D * SCALE;
            meshObj.transform.parent = parent;
            meshObj.transform.localScale = Vector3.one * SCALE;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(pos, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D tex = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.MAP_CHUNK_SIZE, MapGenerator.MAP_CHUNK_SIZE);
            meshRenderer.material.mainTexture = tex;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk() {
            if (!mapDataReceived) return;

            float distFromNearEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
            bool visible = distFromNearEdge <= maxViewDist;

            if (visible) {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++) {
                    if (distFromNearEdge > detailLevels[i].visibleDist) lodIndex = i + 1;
                    else break;
                }

                if (lodIndex != prevLODIndex) {
                    LODMesh lodMesh = lodMeshes[lodIndex];

                    if (lodMesh.hasMesh) {
                        prevLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    } else if (!lodMesh.hasRequested) {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                lastVisibleChunks.Add(this);
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObj.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObj.activeSelf;
        }
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequested;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData) {
            hasRequested = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDist;
    }
}
