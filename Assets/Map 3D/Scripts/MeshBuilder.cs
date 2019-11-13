using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshBuilder : MonoBehaviour {

        Mesh mesh;
        MeshCollider meshCollider;
        public bool useCollider, useColors, useUVCoordinates, useUV2Coordinates;

        [NonSerialized] Vector3[] vertices;
        [NonSerialized] Color[] colors;
        [NonSerialized] Vector2[] uvs;
        [NonSerialized] Vector2[] uv2s;
        [NonSerialized] int[] triangles;
        [NonSerialized] int triangleIndex;
        [NonSerialized] Vector3[] normals;

        Vector3[] borderVertices;
        int[] borderTriangles;
        int borderTriangleIndex;

        int dim = 1;

        void Awake() {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            if (useCollider) {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            mesh.name = "Map Mesh";
        }

        /// <summary>
        /// Set the dimension of the mesh
        /// </summary>
        /// <param name="dim">number of vertices per line</param>
        public void SetDimension(int dim) {
            this.dim = dim;
            Clear();
        }

        #region Triangulation

        /// <summary>
        /// Add a vertex to the mesh
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="uv"></param>
        /// <param name="uv2"></param>
        public void AddVertex(int index, Vector3 position, Color color = new Color(), Vector2 uv = new Vector2(), Vector2 uv2 = new Vector2()) {
            if (index < 0) { // Negative index are ghost vertex used to build normals
                borderVertices[-index - 1] = position;
            }
            else {
                vertices[index] = position;
                colors[index] = color;
                uvs[index] = uv;
                uv2s[index] = uv2;
            }
        }

        /// <summary>
        /// Add a vertex to the mesh
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        public void AddVertex(int index, Vector3 position) {
            if (index < 0) { // Negative index are ghost vertex used to build normals
                borderVertices[-index - 1] = position;
            }
            else {
                vertices[index] = position;
            }
        }

        /// <summary>
        /// Add a color the the given vertex
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <param name="color"></param>
        public void AddVertexColor(int vertexIndex, Color color) {
            if (vertexIndex >= 0) {
                colors[vertexIndex] = color;
            }
        }

        /// <summary>
        /// Add a uv to the given vertex
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <param name="uv"></param>
        public void AddVertexUV(int vertexIndex, Vector2 uv) {
            if (vertexIndex >= 0) {
                uvs[vertexIndex] = uv;
            }
        }

        /// <summary>
        /// Add a uv2 to the given vertex
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <param name="uv"></param>
        public void AddVertexUV2(int vertexIndex, Vector2 uv) {
            if (vertexIndex >= 0) {
                uv2s[vertexIndex] = uv;
            }
        }

        /// <summary>
        /// Add a triangle to the mesh
        /// </summary>
        /// <param name="vertexA"></param>
        /// <param name="vertexB"></param>
        /// <param name="vertexC"></param>
        public void AddTriangle(int vertexA, int vertexB, int vertexC) {
            if (vertexA < 0 || vertexB < 0 || vertexC < 0) {
                borderTriangles[borderTriangleIndex] = vertexA;
                borderTriangles[borderTriangleIndex + 1] = vertexB;
                borderTriangles[borderTriangleIndex + 2] = vertexC;
                borderTriangleIndex += 3;
            }
            else {
                triangles[triangleIndex] = vertexA;
                triangles[triangleIndex + 1] = vertexB;
                triangles[triangleIndex + 2] = vertexC;
                triangleIndex += 3;
            }
        }

        /// <summary>
        /// Add a quadrilatere to the mesh
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="v4"></param>
        public void AddQuad(int v1, int v2, int v3, int v4) {
            AddTriangle(v1, v2, v4);
            AddTriangle(v1, v4, v3);
        }

        #endregion

        #region Normals

        /// <summary>
        /// Recalculate the normals from the vertices and triangles
        /// </summary>
        public void RecalculateNormals() {
            normals = new Vector3[dim * dim];

            // Each vertex normal is the sum of the connected triangle normals
            int triangleCount = triangles.Length / 3;
            for (int i = 0; i < triangleCount; i++) {
                int triangleIndex = i * 3;
                int vertexA = triangles[triangleIndex];
                int vertexB = triangles[triangleIndex + 1];
                int vertexC = triangles[triangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexA, vertexB, vertexC);
                if (vertexA >= 0) { normals[vertexA] += triangleNormal; }
                if (vertexB >= 0) { normals[vertexB] += triangleNormal; }
                if (vertexC >= 0) { normals[vertexC] += triangleNormal; }
            }

            // Do the same with the border triangles
            int borderTriangleCount = borderTriangles.Length / 3;
            for (int i = 0; i < borderTriangleCount; i++) {
                int triangleIndex = i * 3;
                int vertexA = borderTriangles[triangleIndex];
                int vertexB = borderTriangles[triangleIndex + 1];
                int vertexC = borderTriangles[triangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexA, vertexB, vertexC);
                if (vertexA >= 0) { normals[vertexA] += triangleNormal; }
                if (vertexB >= 0) { normals[vertexB] += triangleNormal; }
                if (vertexC >= 0) { normals[vertexC] += triangleNormal; }
            }

            // Normalize normals
            for (int i = 0; i < normals.Length; i++) {
                normals[i].Normalize();
            }
        }

        /// <summary>
        /// Compute a triangle normal based on its vertices index
        /// </summary>
        /// <param name="a">index of vertex a</param>
        /// <param name="b">index of vertex b</param>
        /// <param name="c">index of vertex c</param>
        /// <returns></returns>
        Vector3 SurfaceNormalFromIndices(int a, int b, int c) {
            Vector3 pointA = (a < 0) ? borderVertices[-a - 1] : vertices[a];
            Vector3 pointB = (b < 0) ? borderVertices[-b - 1] : vertices[b];
            Vector3 pointC = (c < 0) ? borderVertices[-c - 1] : vertices[c];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideBC = pointC - pointB;
            return Vector3.Cross(sideAB, sideBC).normalized;
        }

        #endregion

        #region Manage Mesh

        /// <summary>
        /// Clear datas
        /// </summary>
        public void Clear() {
            mesh.Clear();
            vertices = new Vector3[dim * dim];
            if (useColors)
                colors = new Color[dim * dim];
            if (useUVCoordinates)
                uvs = new Vector2[dim * dim];
            if (useUV2Coordinates)
                uv2s = new Vector2[dim * dim];
            triangles = new int[(dim - 1) * (dim - 1) * 6];
            triangleIndex = 0;
            borderVertices = new Vector3[dim * 4 + 4];
            borderTriangles = new int[dim * 24];
            borderTriangleIndex = 0;
        }

        /// <summary>
        /// Apply datas to mesh
        /// </summary>
        public void Apply() {
            RecalculateNormals();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            if (useColors) {
                mesh.colors = colors;
            }
            if (useUVCoordinates) {
                mesh.uv = uvs;
            }
            if (useUV2Coordinates) {
                mesh.uv2 = uv2s;
            }
            mesh.normals = normals;
            if (useCollider) {
                meshCollider.sharedMesh = mesh;
            }
        }

        #endregion
    }
}