using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public static class TextureGenerator {

        public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeightMap(float[,] heightMap, Gradient coloring = null) {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            Color[] colourMap = new Color[width * height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (coloring == null) {
                        colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                    }
                    else {
                        colourMap[y * width + x] = coloring.Evaluate(heightMap[x, y]);
                    }
                }
            }
            return TextureFromColourMap(colourMap, width, height);
        }

        public static Texture2D TextureFromLevels(float[,] map, ColoredLevel[] levels) {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            Color[] colourMap = new Color[width * height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    colourMap[y * width + x] = levels[GetLevel(map[x, y], levels)].color;
                }
            }
            return TextureFromColourMap(colourMap, width, height);
        }

        private static int GetLevel(float value, ColoredLevel[] levels) {
            for (int i = 0; i < levels.Length; i++) {
                if (value <= levels[i].level) {
                    return i;
                }
            }
            return levels.Length - 1;
        }
    }
}