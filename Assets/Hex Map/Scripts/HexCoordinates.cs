using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HexMap {

    [System.Serializable]
    public struct HexCoordinates {

        #region Members

        [SerializeField]
        private int x, z;

        public int X { get { return x; } }
        public int Y { get { return -X - Z; } }
        public int Z { get { return z; } }

        #endregion

        public HexCoordinates(int x, int z) {
            this.x = x;
            this.z = z;
        }


        public static HexCoordinates FromOffsetCoordinates(int x,int z) {
            return new HexCoordinates(x - z / 2, z);
        }


        public static HexCoordinates FromPosition(Vector3 position) {
            float x = position.x / (HexMetrics.innerRadius * 2f);
            float y = -x;
            // shift 1 unit to the left every 2 rows (z axis)
            float offset = position.z / (HexMetrics.outerRadius * 3f); // 1 row = 1.5f * HexMetrics.outerRadius
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY+iZ != 0) {
                // discard the coordinate with the largest rounding delta
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                // reconstruct the discarded coordinate (except if it's iY)
                if (dX > dY && dX > dZ) {
                    iX = -iY - iZ;
                }
                else if (dZ > dY) {
                    iZ = -iX - iY;
                }
            }

            return new HexCoordinates(iX, iZ);
        }

        public int DistanceTo(HexCoordinates other) {
            return ((x < other.x ? other.x - x : x - other.x) +
                (Y < other.Y ? other.Y - Y : Y - other.Y) +
                (z < other.z ? other.z - z : z - other.z)) / 2;
        }


        public override string ToString() {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }


        public string ToStringOnSeparateLines() {
            return X + "\n" + Y + "\n" + Z;
        }

        public void Save(BinaryWriter writer) {
            writer.Write(x);
            writer.Write(z);
        }

        public static HexCoordinates Load (BinaryReader reader) { // A load method does'nt make sense in a struct
            HexCoordinates c;
            c.x = reader.ReadInt32();
            c.z = reader.ReadInt32();
            return c;
        }
    }

}
