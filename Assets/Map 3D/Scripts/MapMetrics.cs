using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    public static class MapMetrics {
        public static int seed = 0;
        public static float scale = 1;
        public static int octaves = 5;
        public static float persistance = 0.5f;
        public static float lacunarity = 1f;
        public static int chunkResolution = 249;
        public static float zoom = 1f;
        public static Gradient heightGradient;
        public static Gradient temperatureGradient;
        public static Gradient moistureGradient;

        public static float amplitude = 5f;

        public static BiomeGraph biomeGraph = null;
    }
}
