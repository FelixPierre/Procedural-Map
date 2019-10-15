using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public class Map3D : MonoBehaviour {

        public int mapSizeX, mapSizeZ;
        int chunkCountX, chunkCountZ;
        public int chunkSize;

        public Chunk chunkPrefab;

        Chunk[] chunks;

        public float scale = 1;
        public int seed;
        public int octaves;
        [Range(0, 1)]
        public float persistance;
        public float lacunarity;
        [Range(1, 255)]
        public int resolution = 249;

        private void Awake() {
            InitMetrics();

            CreateMap();
        }

        void InitMetrics() {
            MapMetrics.scale = scale;
            MapMetrics.octaves = octaves;
            MapMetrics.lacunarity = lacunarity;
            MapMetrics.persistance = persistance;
            MapMetrics.chunkResolution = resolution;
        }

        public void CreateMap() {
            chunkCountX = mapSizeX / chunkSize;
            chunkCountZ = mapSizeZ / chunkSize;

            CreateChunks();
        }

        public void CreateChunks() {
            chunks = new Chunk[chunkCountX * chunkCountZ];

            for (int z = 0, i = 0; z < chunkCountZ; z++) {
                for (int x = 0; x < chunkCountX; x++) {
                    Chunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                    chunk.transform.SetParent(transform);
                    chunk.transform.localPosition = new Vector3(x * chunkSize, 0, z * chunkSize);
                    chunk.Init(new Vector2(x, z), chunkSize, seed);
                }
            }
        }

        protected void OnValidate() {
            if (lacunarity < 1) {
                lacunarity = 1;
            }
            if (octaves < 0) {
                octaves = 0;
            }
            if (resolution % 2 == 0) {
                resolution += 1;
            }
            InitMetrics();
            Refresh();
        }

        void Refresh() {
            if (chunks != null) {
                foreach (Chunk c in chunks) {
                    c.Refresh();
                }
            }
        }
    }
}
