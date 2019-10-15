using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public class Chunk : MonoBehaviour {

        public MeshRenderer noiseRender;
        public MeshCreator terrain;

        [SerializeField]
        Vector2 position;
        int size;
        int seed;

        private void LateUpdate() {
            Triangulate();
            enabled = false;
        }

        public void Refresh() {
            enabled = true;
        }

        public void Init(Vector2 position, int size, int seed) {
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
        }

        void DisplayNoise() {
            noiseRender.transform.localPosition = new Vector3(size / 2, 0, size / 2);
            noiseRender.transform.localScale = new Vector3(-1, 1, 1) * size / 10;

            float[,] noise = Noise.GenerateNoiseMap(MapMetrics.chunkResolution, MapMetrics.chunkResolution, seed, MapMetrics.scale,
                MapMetrics.octaves, MapMetrics.persistance, MapMetrics.lacunarity, position * MapMetrics.chunkResolution);
            Texture2D texture = TextureGenerator.TextureFromHeightMap(noise);
            noiseRender.material.mainTexture = texture;
        }

        void TriangulateTerrain() {
            //terrain.AddQuad(new Vector3(0, 0, 0), new Vector3(size, 0, 0), new Vector3(0, 0, size), new Vector3(size, 0, size));
            //terrain.AddQuadColor(Color.white, Color.red, Color.green, Color.blue);
        }
    }

}
