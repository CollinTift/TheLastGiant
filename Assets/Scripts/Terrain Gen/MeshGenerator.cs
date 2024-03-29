using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail) {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        int meshSimplificationInc = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationInc;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationInc + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        int [,] vertIndexMap = new int[borderedSize, borderedSize];
        int meshVertIndex = 0;
        int borderVertIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationInc) {
            for (int x = 0; x < borderedSize; x += meshSimplificationInc) {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex) {
                    vertIndexMap[x, y] = borderVertIndex;
                    borderVertIndex--;
                } else {
                    vertIndexMap[x, y] = meshVertIndex;
                    meshVertIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y += meshSimplificationInc) {
            for (int x = 0; x < borderedSize; x += meshSimplificationInc) {
                int vertIndex = vertIndexMap[x, y];

                Vector2 percent = new Vector2((x - meshSimplificationInc) / (float)meshSize, (y - meshSimplificationInc) / (float)meshSize);
                float height =  heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertPos = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);
                
                meshData.AddVertex(vertPos, percent, vertIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1) {
                    int a = vertIndexMap[x, y];
                    int b = vertIndexMap[x + meshSimplificationInc, y];
                    int c = vertIndexMap[x, y + meshSimplificationInc];
                    int d = vertIndexMap[x + meshSimplificationInc, y + meshSimplificationInc];
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] borderVerts;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine) {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVerts = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[verticesPerLine * 24];
    }

    public void AddVertex(Vector3 vertPos, Vector2 uv, int vertIndex) {
        if (vertIndex < 0) {
            borderVerts[-vertIndex - 1] = vertPos;
        } else {
            vertices[vertIndex] = vertPos;
            uvs[vertIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c) {
        if (a < 0 || b < 0 || c < 0) {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        } else {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
        
    }

    Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0) vertexNormals[vertexIndexA] += triangleNormal;
            if (vertexIndexB >= 0) vertexNormals[vertexIndexB] += triangleNormal;
            if (vertexIndexC >= 0) vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = (indexA < 0) ? borderVerts[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVerts[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVerts[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = CalculateNormals();
        return mesh;
    }
}
