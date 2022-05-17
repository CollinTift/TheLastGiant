using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 100;
    public int zSize = 100;

    private float offsetX = 100f;
    private float offsetZ = 100f;

    private void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape() {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        offsetX = Random.Range(0f, 99999f);
        offsetZ = Random.Range(0f, 99999f);

        for (int i = 0, z = 0; z < zSize + 1; z++) {
            for (int x = 0; x < xSize +1; x++) {
                float y = Mathf.PerlinNoise(x * .3f + offsetX, z * .3f + offsetZ) * 2f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    // Debugging by drawing gizmos
    // -----------------------------
    // private void OnDrawGizmos() {
    //     if (vertices == null) return;

    //     for (int i = 0; i < vertices.Length; i++) {
    //         Gizmos.DrawSphere(vertices[i], .1f);
    //     }
    // }
}
