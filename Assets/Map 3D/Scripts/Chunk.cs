using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public class Chunk : MonoBehaviour {

        public MeshRenderer heightRenderer;
        public MeshRenderer temperatureRenderer;
        public MeshRenderer moistureRenderer;
        public MeshBuilder terrain;

        [SerializeField]
        Vector2 position;
        float size;
        int seed;

        private float[,] moisture, temperature, heightMap;

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
            heightRenderer.material = new Material(heightRenderer.sharedMaterial);
            temperatureRenderer.material = new Material(temperatureRenderer.sharedMaterial);
            moistureRenderer.material = new Material(moistureRenderer.sharedMaterial);
        }

        void Triangulate() {
            terrain.Clear();
            
            TriangulateTerrain();
            DisplayNoise();

            terrain.Apply();

            //heightRenderer.enabled = false;
        }

        void DisplayNoise() {
            heightRenderer.transform.localPosition = new Vector3(size / 2, -10, size / 2);
            heightRenderer.transform.localScale = new Vector3(-1, 1, -1) * size / 10;
            temperatureRenderer.transform.localPosition = new Vector3(size / 2, -20, size / 2);
            temperatureRenderer.transform.localScale = new Vector3(-1, 1, -1) * size / 10;
            moistureRenderer.transform.localPosition = new Vector3(size / 2, -30, size / 2);
            moistureRenderer.transform.localScale = new Vector3(-1, 1, -1) * size / 10;
            
            Texture2D heightTexture = TextureGenerator.TextureFromLevels(heightMap, MapMetrics.biomeGraph.heights);
            for (int i = 0; i < heightMap.GetLength(0); i++) {
                for (int j = 0; j < heightMap.GetLength(1); j++) {
                    if (heightMap[i,j] == 0) { heightTexture.SetPixel(i, j, Color.red); }
                }
            }
            heightTexture.Apply();
            heightRenderer.material.mainTexture = heightTexture;
            Texture2D temperatureTexture = TextureGenerator.TextureFromLevels(temperature, MapMetrics.biomeGraph.temperatures);
            temperatureRenderer.material.mainTexture = temperatureTexture;
            Texture2D moistureTexture = TextureGenerator.TextureFromLevels(moisture, MapMetrics.biomeGraph.moistures);
            moistureRenderer.material.mainTexture = moistureTexture;
        }

        void TriangulateTerrain() {
            int borderedSize = MapMetrics.chunkResolution + 3;
            terrain.SetDimension(borderedSize - 2);
            GenerateMap();
            //float[,] heightMap = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed, MapMetrics.scale,
            //    MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            //float[,] moisture = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed + 1, MapMetrics.scale,
            //    MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            //float[,] temperature = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed + 2, MapMetrics.scale,
            //    MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);

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
                    Vector3 pos = new Vector3(i, 0, j) * size / MapMetrics.chunkResolution + MapMetrics.amplitude * new Vector3(0, heightMap[i, j], 0);
                    //Color color = MapMetrics.coloring.Evaluate(heighMap[i, j]);
                    Color color = MapMetrics.biomeGraph.GetBiome(temperature[i, j], moisture[i, j], heightMap[i, j]).color;
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

        void GenerateMap() {
            int borderedSize = MapMetrics.chunkResolution + 3;
            float[,] heightMap = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed, MapMetrics.scale,
                MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            float[,] moisture = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed + 1, MapMetrics.scale,
                MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            float[,] temperature = Noise.GenerateNoiseMap(borderedSize, borderedSize, MapMetrics.seed + 2, MapMetrics.scale,
                MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * MapMetrics.chunkResolution);
            //float[,] heightMap = Noise.GenerateNoiseMap2(size, size, borderedSize, borderedSize, MapMetrics.seed, MapMetrics.scale,
            //    MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * size);
            //float[,] moisture = Noise.GenerateNoiseMap2(size, size, borderedSize, borderedSize, MapMetrics.seed + 1, MapMetrics.scale,
            //    MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * size);
            //float[,] temperature = Noise.GenerateNoiseMap2(size, size, borderedSize, borderedSize, MapMetrics.seed + 2, MapMetrics.scale,
            //    MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, MapMetrics.zoom, position * size);

            for (int i = 0; i < borderedSize; i++) {
                for (int j = 0; j < borderedSize; j++) {
                    // moisture
                    //if (heightMap[i, j] <= 0.2) {
                    //    //moisture[i, j] += 8f * heightMap[i, j];
                    //    moisture[i, j] = 1;
                    //}
                    //else if (heightMap[i, j] <= 0.35) {
                    //    moisture[i, j] += 3f * heightMap[i, j];
                    //}
                    //else if (heightMap[i, j] <= 0.4) {
                    //    moisture[i, j] += 1f * heightMap[i, j];
                    //}
                    //else if (heightMap[i, j] <= 0.5) {
                    //    moisture[i, j] += 0.25f * heightMap[i, j];
                    //}
                    moisture[i, j] += MapMetrics.biomeGraph.influenceOnMoisture.Evaluate(heightMap[i, j]);
                    moisture[i, j] = Mathf.Clamp01(moisture[i, j]);

                    // temperature
                    //if (heightMap[i, j] > 0.5) {
                    //    if (heightMap[i, j] <= 0.7) {
                    //        temperature[i, j] -= 0.1f * heightMap[i, j];
                    //    }
                    //    else if (heightMap[i, j] <= 0.8) {
                    //        temperature[i, j] -= 0.2f * heightMap[i, j];
                    //    }
                    //    else if (heightMap[i, j] <= 0.9) {
                    //        temperature[i, j] -= 0.3f * heightMap[i, j];
                    //    }
                    //    else if (heightMap[i, j] <= 1) {
                    //        temperature[i, j] -= 0.4f * heightMap[i, j];
                    //    }
                    //}
                    temperature[i, j] += MapMetrics.biomeGraph.influenceOnTemperature.Evaluate(heightMap[i, j]);
                    temperature[i, j] = Mathf.Clamp01(temperature[i, j]);
                }
            }

            this.heightMap = heightMap;
            this.temperature = temperature;
            this.moisture = moisture;
        }
    }

}
