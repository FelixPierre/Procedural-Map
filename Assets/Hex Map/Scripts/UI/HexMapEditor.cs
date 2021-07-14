using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexMap {

    public class HexMapEditor : MonoBehaviour {
        
        public HexGrid hexGrid;
        int activeTerrainTypeIndex;
        int activeElevation;
        bool applyElevation = true;
        int activeWaterLevel;
        bool applyWaterLevel = true;
        int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;
        bool applyUrbanLevel = true, applyFarmLevel = true, applyPlantLevel = true, applySpecialIndex = true;

        public Material terrainMaterial;

        //bool editMode;

        int brushSize;

        enum OptionalToggle { Ignore, Yes, No }
        OptionalToggle riverMode, roadMode, walledMode;

        bool isDrag;
        HexDirection dragDirection;
        //HexCell previousCell, searchFromCell, searchToCell;
        HexCell previousCell;

        void Awake() {
            terrainMaterial.DisableKeyword("GRID_ON"); // Disable the grid by default
            SetEditMode(false);
        }

        void Update() {
            if (!EventSystem.current.IsPointerOverGameObject()) {
                if (Input.GetMouseButton(0)) {
                    HandleInput();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.U)) {
                    if (Input.GetKey(KeyCode.LeftShift)) {
                        DestroyUnit();
                    }
                    else {
                        CreateUnit();
                    }
                    return;
                }
            }
        }

        HexCell GetCellUnderCursor() {
            return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        }

        void HandleInput() {
            HexCell currentCell = GetCellUnderCursor();
            if (currentCell) {
                if (previousCell && previousCell != currentCell) {
                    ValidateDrag(currentCell);
                }
                else {
                    isDrag = false;
                }
                //if (editMode) {
                EditCells(currentCell);
                //}
                //else if (Input.GetKey(KeyCode.LeftShift) && searchToCell != currentCell) {
                //    if (searchFromCell != currentCell) {
                //        if (searchFromCell) {
                //            searchFromCell.DisableHighlight();
                //        }
                //        searchFromCell = currentCell;
                //        searchFromCell.EnableHighlight(Color.blue);
                //        if (searchToCell) {
                //            hexGrid.FindPath(searchFromCell, searchToCell, 24);
                //        }
                //    }
                //}
                //else if (searchFromCell && searchFromCell != currentCell) {
                //    if (searchToCell != currentCell) {
                //        searchToCell = currentCell;
                //        hexGrid.FindPath(searchFromCell, searchToCell, 24);
                //    }
                //}
                previousCell = currentCell;
            }
            else {
                previousCell = null;
            }
        }

        #region Unit

        void CreateUnit() {
            HexCell cell = GetCellUnderCursor();
            if (cell && !cell.Unit) {
                hexGrid.AddUnit(Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f));
            }
        }

        void DestroyUnit() {
            HexCell cell = GetCellUnderCursor();
            if (cell && cell.Unit) {
                hexGrid.RemoveUnit(cell.Unit);
            }
        }

        #endregion

        void EditCells(HexCell center) {
            int centerX = center.coordinates.X;
            int centerZ = center.coordinates.Z;

            for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
                for (int x = centerX - r; x <= centerX + brushSize; x++) {
                    EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
                }
            }
            for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
                for (int x = centerX - brushSize; x <= centerX + r; x++) {
                    EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
                }
            }
        }

        void EditCell(HexCell cell) {
            if (cell) {
                if (activeTerrainTypeIndex >= 0) {
                    cell.TerrainTypeIndex = activeTerrainTypeIndex;
                }
                if (applyElevation) {
                    cell.Elevation = activeElevation;
                }
                if (applyWaterLevel) {
                    cell.WaterLevel = activeWaterLevel;
                }
                if (riverMode == OptionalToggle.No) {
                    cell.RemoveRiver();
                }
                if (roadMode == OptionalToggle.No) {
                    cell.RemoveRoads();
                }
                if (applyUrbanLevel) {
                    cell.UrbanLevel = activeUrbanLevel;
                }
                if (applyFarmLevel) {
                    cell.FarmLevel = activeFarmLevel;
                }
                if (applyPlantLevel) {
                    cell.PlantLevel = activePlantLevel;
                }
                if (walledMode != OptionalToggle.Ignore) {
                    cell.Walled = walledMode == OptionalToggle.Yes;
                }
                if (applySpecialIndex) {
                    cell.SpecialIndex = activeSpecialIndex;
                }
                if (isDrag) {
                    HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                    if (otherCell) {
                        if (riverMode == OptionalToggle.Yes) {
                            otherCell.SetOutgoingRiver(dragDirection);
                        }
                        if (roadMode == OptionalToggle.Yes) {
                            otherCell.AddRoad(dragDirection);
                        }
                    }
                }
            }
        }

        void ValidateDrag(HexCell currentCell) {
            for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++) {
                if (previousCell.GetNeighbor(dragDirection) == currentCell) {
                    isDrag = true;
                    return;
                }
            }
            isDrag = false;
        }

        public void SetTerrainTypeIndex(int index) {
            activeTerrainTypeIndex = index;
        }

        public void SetElevation(float elevation) {
            activeElevation = (int)elevation;
        }

        public void SetApplyElevation(bool toggle) {
            applyElevation = toggle;
        }

        public void SetBrushSize(float size) {
            brushSize = (int)size;
        }

        public void SetRiverMode(int mode) {
            riverMode = (OptionalToggle)mode;
        }

        public void SetRoadMode(int mode) {
            roadMode = (OptionalToggle)mode;
        }

        public void SetApplyWaterLevel(bool toggle) {
            applyWaterLevel = toggle;
        }

        public void SetWaterLevel(float level) {
            activeWaterLevel = (int)level;
        }

        public void SetApplyUrbanLevel(bool toggle) {
            applyUrbanLevel = toggle;
        }

        public void SetUrbanLevel(float level) {
            activeUrbanLevel = (int)level;
        }

        public void SetApplyFarmLevel(bool toggle) {
            applyFarmLevel = toggle;
        }

        public void SetFarmLevel(float level) {
            activeFarmLevel = (int)level;
        }

        public void SetApplyPlantLevel(bool toggle) {
            applyPlantLevel = toggle;
        }

        public void SetPlantLevel(float level) {
            activePlantLevel = (int)level;
        }

        public void SetWalledMode(int mode) {
            walledMode = (OptionalToggle)mode;
        }

        public void SetApplySpecialIndex(bool toggle) {
            applySpecialIndex = toggle;
        }

        public void SetSpecialIndex(float index) {
            activeSpecialIndex = (int)index;
        }

        public void ShowGrid(bool visible) {
            if (visible) {
                terrainMaterial.EnableKeyword("GRID_ON");
            }
            else {
                terrainMaterial.DisableKeyword("GRID_ON");
            }
        }

        public void SetEditMode(bool toggle) {
            //editMode = toggle;
            //hexGrid.ShowUI(!toggle);
            enabled = toggle;
        }
    }

}
