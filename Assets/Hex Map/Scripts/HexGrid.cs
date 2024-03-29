﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap {

    public class HexGrid : MonoBehaviour {

        public int cellCountX = 20, cellCountZ = 15;
        int chunkCountX, chunkCountZ;

        public HexCell cellPrefab;
        public Text cellLabelPrefab;
        public HexGridChunk chunkPrefab;

        HexCell[] cells;
        HexGridChunk[] chunks;
        
        public Texture2D noiseSource;

        public int seed;

        HexCellPriorityQueue searchFrontier;
        int searchFrontierPhase;
        HexCell currentPathFrom, currentPathTo;
        bool currentPathExists;

        List<HexUnit> units = new List<HexUnit>();
        public HexUnit UnitPrefab;

        public bool HasPath {
            get {
                return currentPathExists;
            }
        }

        void Awake() {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
            HexUnit.unitPrefab = UnitPrefab;
            CreateMap(cellCountX, cellCountZ);
        }

        public bool CreateMap(int x, int z) {
            if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0) {
                Debug.LogError("Unsupported map size.");
                return false;
            }
            ClearPath();
            ClearUnits();
            if (chunks != null) {
                for (int i = 0; i < chunks.Length; i++) {
                    Destroy(chunks[i].gameObject);
                }
            }
            cellCountX = x;
            cellCountZ = z;
            chunkCountX = cellCountX / HexMetrics.chunkSizeX;
            chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;

            CreateChunks();
            CreateCells();
            return true;
        }

        void OnEnable() {
            if (!noiseSource) {
                HexMetrics.noiseSource = noiseSource;
                HexMetrics.InitializeHashGrid(seed);
                HexUnit.unitPrefab = UnitPrefab;
            }
        }

        //void Start() {
        //    hexMesh.Triangulate(cells);
        //}

        void CreateChunks() {
            chunks = new HexGridChunk[chunkCountX * chunkCountZ];

            for (int z = 0, i = 0; z < chunkCountZ; z++) {
                for (int x = 0; x < chunkCountX; x++) {
                    HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }

        void CreateCells() {
            cells = new HexCell[cellCountZ * cellCountX];
            for (int z = 0, i = 0; z < cellCountZ; z++) {
                for (int x = 0; x < cellCountX; x++) {
                    CreateCell(x, z, i++);
                }
            }
        }

        void CreateCell(int x, int z, int i) {
            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f); // offset de 0.5 si z impair, 0 sinon
            position.y = 0f;
            position.z = z * (HexMetrics.outerRadius * 1.5f);

            // Create a cell
            HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
            //cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            // Set neighbors
            if (x > 0) {
                cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            }
            if (z > 0) {
                // even rows
                if ((z & 1) == 0) {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                    if (x > 0) {
                        cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                    }
                }
                // odd rows
                else {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                    if (x < cellCountX - 1) {
                        cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                    }
                }
            }

            // Create debug label with cell's coordinates
            Text label = Instantiate<Text>(cellLabelPrefab);
            //label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            //label.text = cell.coordinates.ToStringOnSeparateLines();
            cell.uiRect = label.rectTransform;
            cell.Elevation = 0; // make sure that the perturbation is applied immediately

            AddCellToChunk(x, z, cell);
        }

        void AddCellToChunk(int x, int z, HexCell cell) {
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;
            HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
        }

        public HexCell GetCell(Vector3 position) {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
            return cells[index];
        }

        public HexCell GetCell(HexCoordinates coordinates) {
            int z = coordinates.Z;
            if (z < 0 || z >= cellCountZ) {
                return null;
            }
            int x = coordinates.X + z / 2;
            if (x<0||x >= cellCountX) {
                return null;
            }
            return cells[x + z * cellCountX];
        }

        public HexCell GetCell(Ray ray) {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                return GetCell(hit.point);
            }
            return null;
        }

        public void ShowUI(bool visible) {
            for (int i = 0; i < chunks.Length; i++) {
                chunks[i].ShowUI(visible);
            }
        }

        public void Save(BinaryWriter writer) {
            writer.Write(cellCountX);
            writer.Write(cellCountZ);

            for (int i = 0; i < cells.Length; i++) {
                cells[i].Save(writer);
            }

            writer.Write(units.Count);
            for (int i = 0; i < units.Count; i++) {
                units[i].Save(writer);
            }
        }

        public void Load(BinaryReader reader, int header) {
            ClearPath();
            ClearUnits();
            int x = 30, z = 25;
            if (header >= 1) {
                x = reader.ReadInt32();
                z = reader.ReadInt32();
            }
            if (x != cellCountX && z != cellCountZ) {
                if (!CreateMap(x, z)) {
                    return;
                }
            }

            for (int i = 0; i < cells.Length; i++) {
                cells[i].Load(reader);
            }
            for (int i = 0; i < chunks.Length; i++) {
                chunks[i].Refresh();
            }

            if (header >= 2) { // since save file version 2
                int unitCount = reader.ReadInt32();
                for (int i = 0; i < unitCount; i++) {
                    HexUnit.Load(reader, this);
                }
            }
        }

        #region Path

        public void FindPath(HexCell fromCell, HexCell toCell, int speed) {
            ClearPath();
            currentPathFrom = fromCell;
            currentPathTo = toCell;
            currentPathExists = Search(fromCell, toCell, speed);
            ShowPath(speed);
        }

        bool Search(HexCell fromCell, HexCell toCell, int speed) {
            searchFrontierPhase += 2;
            if (searchFrontier == null) {
                searchFrontier = new HexCellPriorityQueue();
            }
            else {
                searchFrontier.Clear();
            }
            fromCell.SearchPhase = searchFrontierPhase;
            fromCell.Distance = 0;
            searchFrontier.Enqueue(fromCell);
            while (searchFrontier.Count > 0) {
                HexCell current = searchFrontier.Dequeue();
                current.SearchPhase += 1;

                // Stop when current cell is the destination
                if (current == toCell) {
                    return true;
                }

                int currentTurn = current.Distance / speed;

                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
                    HexCell neighbor = current.GetNeighbor(d);
                    // Conditions that make neighbor unreachable
                    if (neighbor == null || neighbor.SearchPhase>searchFrontierPhase) {
                        continue;
                    }
                    if (neighbor.IsUnderwater || neighbor.Unit) {
                        continue;
                    }
                    HexEdgeType edgeType = current.GetEdgeType(neighbor);
                    if (edgeType == HexEdgeType.Cliff) {
                        continue;
                    }
                    // Calculate movement cost based on features
                    int moveCost;
                    if (current.HasRoadThroughEdge(d)) {
                        moveCost = 1;
                    }
                    else if (current.Walled != neighbor.Walled) {
                        continue;
                    }
                    else {
                        moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
                        moveCost += neighbor.UrbanLevel + neighbor.FarmLevel + neighbor.PlantLevel;
                    }

                    int distance = current.Distance + moveCost;
                    int turn = distance / speed;
                    // If you need a new turn to reach the new cell, take wasted movement points into account
                    if (turn > currentTurn) {
                        distance = turn * speed + moveCost;
                    }

                    if (neighbor.SearchPhase < searchFrontierPhase) {
                        neighbor.SearchPhase = searchFrontierPhase;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;
                        neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates); // Heuristic
                        searchFrontier.Enqueue(neighbor);
                    }
                    else if (distance < neighbor.Distance) {
                        int oldPriority = neighbor.SearchPriority;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;
                        searchFrontier.Change(neighbor, oldPriority);
                    }
                }
            }

            return false;
        }

        void ShowPath(int speed) {
            if (currentPathExists) {
                HexCell current = currentPathTo;
                while (current != currentPathFrom) {
                    int turn = current.Distance / speed;
                    current.SetLabel(turn.ToString());
                    current.EnableHighlight(Color.white);
                    current = current.PathFrom;
                }
            }
            currentPathFrom.EnableHighlight(Color.blue);
            currentPathTo.EnableHighlight(Color.red);
        }

        public void ClearPath() {
            if (currentPathExists) {
                HexCell current = currentPathTo;
                while (current != currentPathFrom) {
                    current.SetLabel(null);
                    current.DisableHighlight();
                    current = current.PathFrom;
                }
                current.DisableHighlight();
                currentPathExists = false;
            }
            else if (currentPathFrom) {
                currentPathFrom.DisableHighlight();
                currentPathTo.DisableHighlight();
            }
            currentPathFrom = currentPathTo = null;
        }

        #endregion

        #region Units

        void ClearUnits() {
            for (int i = 0; i < units.Count; i++) {
                units[i].Die();
            }
            units.Clear();
        }

        public void AddUnit(HexUnit unit, HexCell location, float orientation) {
            units.Add(unit);
            unit.transform.SetParent(transform, false);
            unit.Location = location;
            unit.Orientation = orientation;
        }

        public void RemoveUnit(HexUnit unit) {
            units.Remove(unit);
            unit.Die();
        }

        #endregion
    }

}