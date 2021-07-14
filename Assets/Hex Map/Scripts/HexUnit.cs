using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HexMap {

    public class HexUnit : MonoBehaviour {

        public static HexUnit unitPrefab; // for loading purpose

        HexCell location;
        float orientation;

        public HexCell Location {
            get {
                return location;
            }
            set {
                if (location) {
                    location.Unit = null; // Remove unit from old cell
                }
                location = value;
                value.Unit = this;
                transform.localPosition = value.Position;
            }
        }

        public float Orientation {
            get {
                return orientation;
            }
            set {
                orientation = value;
                transform.localRotation = Quaternion.Euler(0f, value, 0f);
            }
        }

        public void ValidateLocation() {
            transform.localPosition = location.Position;
        }

        public void Die() {
            location.Unit = null;
            Destroy(gameObject);
        }

        public void Save(BinaryWriter writer) {
            location.coordinates.Save(writer);
            writer.Write(orientation);
        }

        public static void Load (BinaryReader reader, HexGrid grid) {
            HexCoordinates coordinates = HexCoordinates.Load(reader);
            float orientation = reader.ReadSingle(); // Float are single-precision floating-point numbers compare to double that are double-precision floating-point numbers
            grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
        }

        public bool IsValideDestination(HexCell cell) {
            return !cell.IsUnderwater && !cell.Unit;
        }
    }
}