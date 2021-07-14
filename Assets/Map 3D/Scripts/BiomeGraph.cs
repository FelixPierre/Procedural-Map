using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public class BiomeGraph : MonoBehaviour {

        public ColoredLevel[] temperatures;
        public ColoredLevel[] moistures;
        public ColoredLevel[] heights;

        public float seaLevel;
        public Biome[] biomes;
        public Biome[] oceans;
        [SerializeField] private Texture2D biomeTexture;

        public AnimationCurve influenceOnTemperature;
        public AnimationCurve influenceOnMoisture;

        // Start is called before the first frame update
        void Start() {
            MapMetrics.biomeGraph = this;
        }

        // Update is called once per frame
        void Update() {

        }

        public Biome GetBiome(float temperature, float moisture, float height) {
            if (height <= seaLevel) {
                int x = GetLevel(temperature, temperatures);

                return oceans[x];
            }
            else {
                int x = GetLevel(temperature, temperatures);
                int y = GetLevel(moisture, moistures);

                return biomes[y * temperatures.Length + x];
            }
        }

        private int GetLevel(float value, ColoredLevel[] levels) {
            for (int i = 0; i < levels.Length; i++) {
                if (value <= levels[i].level) {
                    return i;
                }
            }
            return levels.Length - 1;
        }

        Texture2D TextureFromBiomes() {
            int width = temperatures.Length;
            int height = moistures.Length;

            Color[] colourMap = new Color[width * height];
            for (int i = 0; i < width * height; i++) {
                colourMap[i] = biomes[i].color;
            }

            Texture2D texture = new Texture2D(temperatures.Length, moistures.Length);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        private void OnValidate() {
            biomeTexture = TextureFromBiomes();
        }
    }

    [Serializable]
    public class ColoredLevel {
        public float level;
        public Color color;

        public ColoredLevel(float level, Color color) {
            this.level = level;
            this.color = color;
        }
    }
}