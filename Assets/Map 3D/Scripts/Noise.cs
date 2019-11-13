﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public static class Noise {

        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float zoom, Vector2 offset) {
            float[,] noiseMap = new float[mapWidth, mapHeight];
            offset /= zoom;

            System.Random prng = new System.Random(seed);
            Vector2[] octavesOffsets = new Vector2[octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < octaves; i++) {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octavesOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            if (scale <= 0) {
                scale = 0.0001f;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            //float halfWidth = mapWidth / 2f;
            //float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {

                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++) {
                        float sampleX = (x / zoom /*- halfWidth*/  + octavesOffsets[i].x) / scale * frequency;
                        float sampleY = (y / zoom /*- halfHeight*/ + octavesOffsets[i].y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight) {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight) {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }

            return noiseMap;
        }

        //public static float[,] GenerateNoiseMap3(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        //    float[,] noiseMap = new float[mapWidthRes, mapHeightRes];

        //    System.Random prng = new System.Random(seed);
        //    Vector2[] octavesOffsets = new Vector2[octaves];

        //    float maxPossibleHeight = 0;
        //    float amplitude = 1;
        //    float frequency = 1;

        //    for (int i = 0; i < octaves; i++) {
        //        float offsetX = prng.Next(-100000, 100000) + offset.x;
        //        float offsetY = prng.Next(-100000, 100000) - offset.y;
        //        octavesOffsets[i] = new Vector2(offsetX, offsetY);

        //        maxPossibleHeight += amplitude;
        //        amplitude *= persistance;
        //    }

        //    if (scale <= 0) {
        //        scale = 0.0001f;
        //    }

        //    float maxLocalNoiseHeight = float.MinValue;
        //    float minLocalNoiseHeight = float.MaxValue;

        //    //float halfWidth = mapWidth / 2f;
        //    //float halfHeight = mapHeight / 2f;

        //    for (int y = 0; y < mapHeightRes; y++) {
        //        for (int x = 0; x < mapWidthRes; x++) {

        //            amplitude = 1;
        //            frequency = 1;
        //            float noiseHeight = 0;

        //            for (int i = 0; i < octaves; i++) {
        //                float sampleX = (x * (mapWidth / mapWidthRes) /*- halfWidth*/  + octavesOffsets[i].x) / scale * frequency;
        //                float sampleY = (y * (mapHeight / mapHeightRes) /*- halfHeight*/ + octavesOffsets[i].y) / scale * frequency;

        //                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
        //                noiseHeight += perlinValue * amplitude;

        //                amplitude *= persistance;
        //                frequency *= lacunarity;
        //            }

        //            if (noiseHeight > maxLocalNoiseHeight) {
        //                maxLocalNoiseHeight = noiseHeight;
        //            }
        //            else if (noiseHeight < minLocalNoiseHeight) {
        //                minLocalNoiseHeight = noiseHeight;
        //            }
        //            noiseMap[x, y] = noiseHeight;
        //        }
        //    }

        //    for (int y = 0; y < mapHeight; y++) {
        //        for (int x = 0; x < mapWidth; x++) {
        //            float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);
        //            noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
        //        }
        //    }

        //    return noiseMap;
        //}
    }
}
