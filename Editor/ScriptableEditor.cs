using System;
using System.Collections.Generic;
using DataAsset.Core.Struct;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DataAsset.Editor
{
      [CustomEditor(typeof(DataAsset.Core.DataAssetSo))]
      public sealed partial class ScriptableEditor : UnityEditor.Editor
      {
            // Constants for layout and styles
            private const float HeaderSpacing = 5f;
            private const float HeaderRightMargin = 10f;
            private const float HeaderSearchFieldMinWidth = 100f;
            private const float HeaderSearchFieldMaxWidthPercentage = 0.1f;
            private const float HeaderSortButtonWidth = 60f;
            private const float HeaderScanButtonWidth = 90f;
            private const float SectionSeparatorSpace = 15f;
            private const float AddSectionTopSpace = 5f;

            // Constants for element layout
            private const float ElementColorBarWidth = 4f;
            private const float ElementDragHandleWidth = 10f;
            private const float ElementContentPadding = 15f;
            private const float ElementNameLabelWidth = 40f;
            private const float ElementMinNameFieldWidth = 50f;
            private const float ElementTypePadding = 5f;
            private const float ElementUsageFoldoutMinWidth = 60f;
            private const float ElementValueLabelWidth = 45f;

            // Constants for spacing
            private const float SmallVerticalSpacing = 2f;

            // The target assetSo being edited
            private DataAsset.Core.DataAssetSo _targetAssetSo;

            // Serialized property for the list of data objects
            private SerializedProperty _allDataProperty;

            // Reorderable list for managing the data objects
            private ReorderableList _reorderableList;

            // Reflection data for data types and their display names
            private Type[] _dataTypes;
            private string[] _dataTypeDisplayNames;

            // Dictionary to hold colors for each data type
            private readonly Dictionary<Type, Color> _typeColors = new();

            // Search text for filtering data objects
            private string _searchText = "";

            // Pending data for adding new already defined data objects
            private Type _pendingType;
            private string _pendingName = "";
            private int _pendingIntValue;
            private string _pendingStringValue = "";
            private bool _pendingBoolValue;
            private short _pendingShortValue;
            private byte _pendingByteValue;
            private float _pendingFloatValue;
            private long _pendingLongValue;

            // Scanning usage
            private readonly Dictionary<string, List<UsageInfo>> _detailedDataUsages = new();
            private readonly Dictionary<int, bool> _foldoutUsageStates = new();
            private bool _isScanningUsages;
            private EditorCoroutine _scanCoroutine;

            // Flag to check if styles have been initialized
            private bool _stylesInitialized;

            private const string UsageCacheFolderName = "ScriptableAssetUsageCache";

            /// <summary>
            /// Unity calls this method automatically when the custom editor is enabled in the Inspector.
            /// It initializes important serialized properties, validates assetSo data, and sets up custom
            /// editor functionality like data reflection, styles, and UI components.
            /// </summary>
            /// <remarks>
            /// - The method ensures the target ScriptableObject is properly assigned and logs an error if null.
            /// - Finds and validates the serialized property for assetSo data to ensure it is accessible for modification.
            /// - Calls helper methods to initialize type reflection systems, define custom colors, configure reorderable lists,
            /// and reset pending data for the editor.
            /// - If any property is incorrectly set or missing, appropriate error messages are logged in the console.
            /// </remarks>
            private void OnEnable()
            {
                  // Initialize the target assetSo and serialized properties
                  _targetAssetSo = (DataAsset.Core.DataAssetSo)this.target;

                  if (!_targetAssetSo)
                  {
                        Debug.LogError("[ScriptableEditor] Target assetSo is null. Ensure the scriptable object is assigned correctly.");

                        return;
                  }

                  // Find the serialized property for the assetSo data
                  _allDataProperty = this.serializedObject.FindProperty("dataList");

                  // Check if the property is found and is an array/list
                  if (_allDataProperty is not { isArray: true })
                  {
                        Debug.LogError($"[ScriptableEditor] SerializedProperty 'dataList' not found or is not a list/array in '{_targetAssetSo.name}'.");
                        _allDataProperty = null;

                        return;
                  }

                  // Initialize editor styles and data type reflection
                  InitializeDataTypeReflectionAndColors();
                  SetupReorderableList();
                  ResetPendingData();
                  LoadUsageCache();
            }

            /// <summary>
            /// Renders and manages the Inspector interface for the associated ScriptableObject.
            /// It integrates custom UI components, handles data validation, and processes
            /// serialized properties for display and modification within the Editor.
            /// </summary>
            /// <remarks>
            /// - Updates the serialized object to ensure all data is synchronized between the assetSo and the editor.
            /// - Displays errors or warnings if required serialized properties or UI elements are unavailable.
            /// - Incorporates a reorderable list to enhance the user experience when managing large datasets.
            /// - Includes mechanisms to add new data entries via a custom "Add Data" section.
            /// - Applies modifications to serialized properties, triggering data validation when needed to ensure
            /// consistent and error-free assetSo state.
            /// - When a layout event occurs, invokes validation of all names to ensure unique and error-free identifiers.
            /// - Provides fallback behavior to render default inspector elements when critical data is unavailable.
            /// </remarks>
            public override void OnInspectorGUI()
            {
                  InitializeEditorStyles();
                  this.serializedObject.Update();

                  if (_allDataProperty == null)
                  {
                        EditorGUILayout.HelpBox("Failed to initialize data property ('dataList').", MessageType.Error);
                        DrawDefaultInspector();

                        return;
                  }

                  if (Event.current.type == EventType.Layout)
                  {
                        ValidateAllNames();
                  }

                  if (_allDataProperty.arraySize == 1)
                  {
                        DrawSingleDataObjectInspector();
                  }
                  else
                  {
                        if (_reorderableList != null)
                        {
                              _reorderableList.DoLayoutList();
                              DrawClearAllButton();
                        }
                        else
                        {
                              EditorGUILayout.HelpBox("Data list UI could not be initialized.", MessageType.Warning);
                              EditorGUILayout.PropertyField(_allDataProperty, true);
                        }

                        DrawAddDataSectionLayout();
                  }

                  if (this.serializedObject.ApplyModifiedProperties())
                  {
                        ValidateAllNames();
                  }
            }
      }
}