﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexMap {

    public class HexGameUI : MonoBehaviour {
        public HexGrid grid;

        HexCell currentCell;

        HexUnit selectedUnit;

        void Update() {
            // If cursor not on top of GUI element
            if (!EventSystem.current.IsPointerOverGameObject()) {
                if (Input.GetMouseButtonDown(0)) {
                    DoSelection();
                }
                else if (selectedUnit) {
                    if (Input.GetMouseButtonDown(1)) {
                        DoMove();
                    }
                    else {
                        DoPathFinding();
                    }
                }
            }
        }

        public void SetEditMode (bool toggle) {
            enabled = !toggle;
            grid.ShowUI(!toggle);
            grid.ClearPath();
        }

        bool UpdateCurrentCell() {
            HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (cell != currentCell) {
                currentCell = cell;
                return true;
            }
            return false;
        }

        void DoSelection() {
            grid.ClearPath();
            UpdateCurrentCell();
            if (currentCell) {
                selectedUnit = currentCell.Unit;
            }
        }

        void DoPathFinding() {
            if (UpdateCurrentCell()) {
                if (currentCell && selectedUnit.IsValideDestination(currentCell)) {
                    grid.FindPath(selectedUnit.Location, currentCell, 24);
                }
                else {
                    grid.ClearPath();
                }
            }
        }

        void DoMove() {
            if (grid.HasPath) {
                selectedUnit.Location = currentCell;
                grid.ClearPath();
            }
        }
    }
}
