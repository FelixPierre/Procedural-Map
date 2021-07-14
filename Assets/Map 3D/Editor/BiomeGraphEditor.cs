using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Map3d {

    [CustomEditor(typeof(BiomeGraph))]
    public class BiomeGraphEditor : Editor {

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            serializedObject.Update();
            SerializedProperty temperatures = serializedObject.FindProperty("temperatures");
            SerializedProperty moistures = serializedObject.FindProperty("moistures");
            SerializedProperty biomes = serializedObject.FindProperty("biomes");
            SerializedProperty oceans = serializedObject.FindProperty("oceans");

            biomes.arraySize = temperatures.arraySize * moistures.arraySize;
            oceans.arraySize = temperatures.arraySize;

            EditorGUILayout.PropertyField(temperatures, true);
            EditorGUILayout.PropertyField(moistures, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("heights"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("seaLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("influenceOnTemperature"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("influenceOnMoisture"));

            serializedObject.ApplyModifiedProperties();
            BiomeDrawer(biomes, temperatures.arraySize, moistures.arraySize);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(serializedObject.FindProperty("biomeTexture").objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(140), GUILayout.Height(140));
            GUI.enabled = true;
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("biomeTexture"));
            OceanDrawer(oceans);
            serializedObject.ApplyModifiedProperties();
        }

        public void BiomeDrawer(SerializedProperty biomes, int tempSize, int MoisSize) {
            EditorGUILayout.PropertyField(biomes);
            if (biomes.isExpanded) {
                for (int i = MoisSize - 1; i >= 0; i--) {
                    Rect position = EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < tempSize; j++) {
                        EditorGUILayout.PropertyField(biomes.GetArrayElementAtIndex(i * tempSize + j), GUIContent.none);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        public void OceanDrawer(SerializedProperty oceans) {
            EditorGUILayout.PropertyField(oceans);
            EditorGUI.indentLevel++;
            if (oceans.isExpanded) {
                for (int i = 0; i < oceans.arraySize; i++) {
                    EditorGUILayout.PropertyField(oceans.GetArrayElementAtIndex(i), new GUIContent("Temperature " + i));
                }
            }
            EditorGUI.indentLevel--;
        }
    }


    public static class CustomList {
        public static void Show(SerializedProperty list) {
            EditorGUILayout.PropertyField(list);
            EditorGUI.indentLevel++;
            if (list.isExpanded) {
                for (int i = 0; i < list.arraySize; i++) {
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
                }
            }
            EditorGUI.indentLevel--;
        }
    }

    [CustomPropertyDrawer(typeof(ColoredLevel))]
    public class ColoredLevelDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            int oldIndent = EditorGUI.indentLevel;
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            if (position.height > 16f) {
                position.height = 16f;
                EditorGUI.indentLevel += 1;
                contentPosition = EditorGUI.IndentedRect(position);
                contentPosition.y += 18f;
            }
            contentPosition.width *= 0.5f;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 14f;
            EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("level"), new GUIContent("L"));
            contentPosition.x += contentPosition.width;
            EditorGUIUtility.labelWidth = 14f;
            EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("color"), new GUIContent("C"));
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = oldIndent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return Screen.width < 333 ? (16f + 18f) : 16f;
        }
    }

}
