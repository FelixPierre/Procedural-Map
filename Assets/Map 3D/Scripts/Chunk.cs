using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public class Chunk : MonoBehaviour {

        public MeshRenderer noiseRender;
        public MeshBuilder terrain;

        [SerializeField]
        Vector2 position;
        float size;
        int seed;

        private void LateUpdate() {
            Triangulate();
            enabled = false;
        }

        public void Refresh() {
            enabled = true;
        }

        public void Init(Vector2 position, float size, int seed) {
            this.position = position;
            this.size = size;
            this.seed = seed;
            noiseRender.material = new Material(noiseRender.sharedMaterial);
        }

        void Triangulate() {
            terrain.Clear();

            DisplayNoise();
            TriangulateTerrain();

            terrain.Apply();

            noiseRender.enabled = false;
        }

        void DisplayNoise() {
            noiseRender.transform.localPosition = new Vector3(size / 2, 0, size / 2);
            noiseRender.transform.localScale = new Vector3(-1, 1, -1) * size / 10;

            float[,] noise = Noise.GenerateNoiseMap(MapMetrics.chunkResolution, MapMetrics.chunkResolution, MapMetrics.seed, MapMetrics.scale,
                MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            Texture2D texture = TextureGenerator.TextureFromHeightMap(noise, MapMetrics.coloring);
            noiseRender.material.mainTexture = texture;
        }

        void TriangulateTerrain() {
            int borderedSize = MapMetrics.chunkResolution + 3;
            terrain.SetDimension(borderedSize - 2);
            float[,] noise = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed, MapMetrics.scale,
                MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            
            int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
            int borderIndex = -1;
            int vertexIndex = 0;
            for (int i = 0; i < borderedSize; i++) {
                for (int j = 0; j < borderedSize; j++) {
                    // Define vertex indices
                    bool isOnBorder = i == 0 || i == borderedSize - 1 || j == 0 || j == borderedSize - 1;
                    int index;
                    if (isOnBorder) {
                        index = vertexIndicesMap[i, j] = borderIndex;
                        borderIndex--;
                    }
                    else {
                        index = vertexIndicesMap[i, j] = vertexIndex;
                        vertexIndex++;
                    }

                    // Add vertex to mesh
                    Vector3 pos = new Vector3(i, 0, j) * size / MapMetrics.chunkResolution + MapMetrics.amplitude * new Vector3(0, noise[i, j], 0);
                    Color color = MapMetrics.coloring.Evaluate(noise[i, j]);
                    terrain.AddVertex(index, pos);
                    terrain.AddVertexColor(index, color);
                }
            }

            // Add triangles to mesh
            for (int i = 0; i < borderedSize - 1; i++) {
                for (int j = 0; j < borderedSize - 1; j++) {
                    int v1 = vertexIndicesMap[i, j];
                    int v2 = vertexIndicesMap[i + 1, j];
                    int v3 = vertexIndicesMap[i, j + 1];
                    int v4 = vertexIndicesMap[i + 1, j + 1];
                    
                    terrain.AddTriangle(v1, v4, v2);
                    terrain.AddTriangle(v1, v3, v4);
                }
            }
        }
    }

}
