using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map3d {

    [CreateAssetMenu(fileName = "Biome", menuName = "Map/Biome", order = 1)]
    public class Biome : ScriptableObject {
        public new string name;
        public Color color;
    }

}
