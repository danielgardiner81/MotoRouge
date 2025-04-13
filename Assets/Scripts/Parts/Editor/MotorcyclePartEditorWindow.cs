using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Parts.Components;
using Parts.Editor.Drawers;

namespace Parts.Editor
{
    public class MotorcyclePartEditorWindow : OdinEditorWindow
    {
        [MenuItem("Motorcycle/Part Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<MotorcyclePartEditorWindow>();
            window.titleContent = new GUIContent("Motorcycle Part Editor");
            window.minSize = new Vector2(800, 600);
        }

        [SerializeField]
        private PartDefinition selectedPart;

        private UnityEditor.Editor partEditor;
        private Vector2 listScrollPosition;

        [HideLabel]
        [OnInspectorGUI("DrawCustomList")]
        [SerializeField, PropertySpace(0, 10)]
        private List<PartDefinition> availableParts = new();

        private void DrawCustomList()
        {
            SirenixEditorGUI.BeginBox("Available Parts");
            {
                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);

                for (int i = 0; i < availableParts.Count; i++)
                {
                    var part = availableParts[i];
                    if (part == null) continue;

                    // Begin the selectable row
                    var rowRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        // Add some padding
                        GUILayout.Space(5);

                        // Draw part icon
                        var iconRect = GUILayoutUtility.GetRect(20, 20);
                        GUI.DrawTexture(iconRect, AssetPreview.GetMiniThumbnail(part));

                        GUILayout.Space(5);

                        // Draw part name
                        EditorGUILayout.LabelField(part.name, EditorStyles.boldLabel);

                        // Optional: Draw additional part info
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(part.GetType().Name, EditorStyles.miniLabel);
                        GUILayout.Space(5);
                    }
                    EditorGUILayout.EndHorizontal();

                    // Handle selection
                    if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                    {
                        SelectPart(part);
                        Event.current.Use();
                    }

                    // Draw selection highlight
                    if (part == selectedPart)
                    {
                        EditorGUI.DrawRect(rowRect, new Color(0.239f, 0.501f, 0.866f, 0.2f));
                    }

                    // Draw hover highlight
                    if (rowRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(rowRect, new Color(1, 1, 1, 0.1f));
                        Repaint();
                    }
                }

                EditorGUILayout.EndScrollView();
            }
            SirenixEditorGUI.EndBox();

            // Draw buttons
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Create New Part", EditorStyles.miniButtonLeft))
                {
                    CreateNewPart();
                }

                GUI.enabled = selectedPart != null;
                if (GUILayout.Button("Delete Selected Part", EditorStyles.miniButtonRight))
                {
                    DeleteSelectedPart();
                }

                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadAvailableParts();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (partEditor != null)
                DestroyImmediate(partEditor);
        }

        private void LoadAvailableParts()
        {
            var guids = AssetDatabase.FindAssets("t:PartDefinition");
            availableParts = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<PartDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(part => part != null)
                .ToList();
        }

        private void SelectPart(PartDefinition part)
        {
            if (partEditor != null)
                DestroyImmediate(partEditor);

            selectedPart = part;

            if (part != null)
                partEditor = UnityEditor.Editor.CreateEditor(part);
        }

        [OnInspectorGUI]
        private void DrawSelectedPart()
        {
            if (selectedPart == null) return;

            SirenixEditorGUI.BeginHorizontalPropertyLayout(GUIContent.none);
            {
                // Left side - Part Editor
                SirenixEditorGUI.BeginVerticalPropertyLayout(GUIContent.none);
                {
                    if (partEditor != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        partEditor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(selectedPart);
                        }
                    }
                }
                SirenixEditorGUI.EndVerticalPropertyLayout();
            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        private void CreateNewPart()
        {
            var part = CreateInstance<PartDefinition>();
            var path = EditorUtility.SaveFilePanelInProject(
                "Save New Part",
                "NewPart",
                "asset",
                "Please enter a name for the new part"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(part, path);
                AssetDatabase.SaveAssets();
                LoadAvailableParts();
                SelectPart(part);
            }
        }

        private void DeleteSelectedPart()
        {
            if (selectedPart == null) return;

            if (EditorUtility.DisplayDialog("Delete Part",
                    $"Are you sure you want to delete {selectedPart.name}?",
                    "Delete",
                    "Cancel"))
            {
                var path = AssetDatabase.GetAssetPath(selectedPart);
                AssetDatabase.DeleteAsset(path);
                SelectPart(null);
                LoadAvailableParts();
            }
        }
    }
}