using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public class MapGenerator : MonoBehaviour {

        public Transform mesh;

        // Start is called before the first frame update
        void Start() {
            
        }

        void GenerateMapData(Vector2 centre) {
            float[,] noiseMap = GenerateNoiseMap(10, 10, 0);

            MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap);

            mesh.GetComponent<MeshFilter>().mesh = meshData.CreateMesh();
            //mesh.transform.localScale = Vector3.one;
        }

        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed) {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(seed);

            //float maxLocalNoiseHeight = float.MinValue;
            //float minLocalNoiseHeight = float.MaxValue;

            //float halfWidth = mapWidth / 2f;
            //float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {

                    float noiseHeight = 0;

                    //if (noiseHeight > maxLocalNoiseHeight) {
                    //    maxLocalNoiseHeight = noiseHeight;
                    //}
                    //else if (noiseHeight < minLocalNoiseHeight) {
                    //    minLocalNoiseHeight = noiseHeight;
                    //}
                    noiseMap[x, y] = noiseHeight;
                }
            }

            //for (int y = 0; y < mapHeight; y++) {
            //    for (int x = 0; x < mapWidth; x++) {
            //        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
            //    }
            //}

            return noiseMap;
        }
    }


    public struct MapData {
        public readonly float[,] heightMap;

        public MapData(float[,] heightMap) {
            this.heightMap = heightMap;
        }
    }
}
