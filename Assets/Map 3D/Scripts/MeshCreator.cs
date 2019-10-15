using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshCreator : MonoBehaviour {

        Mesh mesh;
        MeshCollider meshCollider;
        public bool useCollider, useColors, useUVCoordinates, useUV2Coordinates;

        [NonSerialized] List<Vector3> vertices = new List<Vector3>();
        [NonSerialized] List<int> triangles = new List<int>();
        [NonSerialized] List<Color> colors = new List<Color>();
        [NonSerialized] List<Vector2> uvs = new List<Vector2>();
        [NonSerialized] List<Vector2> uv2s = new List<Vector2>();

        void Awake() {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            if (useCollider) {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            mesh.name = "Hex Mesh";
        }

        #region Triangles

        // Create a triangle without perturbations
        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }


        // Single color triangle
        public void AddTriangleColor(Color color) {
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
        }


        // 3-colored triangle
        public void AddTriangleColor(Color c1, Color c2, Color c3) {
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
        }

        public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector3 uv3) {
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
        }

        public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3) {
            uv2s.Add(uv1);
            uv2s.Add(uv2);
            uv2s.Add(uv3);
        }

        #endregion

        #region Quads

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }


        // 4-colored quad
        public void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
            colors.Add(c4);
        }


        // 2-colored quad
        public void AddQuadColor(Color c1, Color c2) {
            colors.Add(c1);
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c2);
        }

        // 1-colored quad
        public void AddQuadColor(Color color) {
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
        }

        public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
            uvs.Add(uv4);
        }

        public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
            uvs.Add(new Vector2(uMin, vMin));
            uvs.Add(new Vector2(uMax, vMin));
            uvs.Add(new Vector2(uMin, vMax));
            uvs.Add(new Vector2(uMax, vMax));
        }

        public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
            uv2s.Add(uv1);
            uv2s.Add(uv2);
            uv2s.Add(uv3);
            uv2s.Add(uv4);
        }

        public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
            uv2s.Add(new Vector2(uMin, vMin));
            uv2s.Add(new Vector2(uMax, vMin));
            uv2s.Add(new Vector2(uMin, vMax));
            uv2s.Add(new Vector2(uMax, vMax));
        }

        #endregion

        // Clear datas
        public void Clear() {
            mesh.Clear();
            vertices = new List<Vector3>();
            triangles = new List<int>();
            if (useColors) {
                colors = new List<Color>();
            }
            if (useUVCoordinates) {
                uvs = new List<Vector2>();
            }
            if (useUV2Coordinates) {
                uv2s = new List<Vector2>();
            }
        }

        // Apply datas to mesh
        public void Apply() {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            if (useColors) {
                mesh.SetColors(colors);
            }
            if (useUVCoordinates) {
                mesh.SetUVs(0, uvs);
            }
            if (useUV2Coordinates) {
                mesh.SetUVs(1, uv2s);
            }
            mesh.RecalculateNormals();
            if (useCollider) {
                meshCollider.sharedMesh = mesh;
            }
        }
    }

}