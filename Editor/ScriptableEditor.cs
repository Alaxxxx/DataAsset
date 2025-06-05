using System;
using System.Collections.Generic;
using ScriptableAsset.Core;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      [CustomEditor(typeof(ScriptableAsset))]
      public sealed partial class ScriptableEditor : UnityEditor.Editor
      {
            // The target asset being edited
            private ScriptableAsset _targetAsset;

            // Serialized property for the list of data objects
            private SerializedProperty _allDataProperty;

            // Reorderable list for managing the data objects
            private ReorderableList _reorderableList;

            // Reflection data for data types and their display names
            private Type[] _dataTypes;
            private string[] _dataTypeDisplayNames;

            // Dictionary to hold colors for each data type
            private readonly Dictionary<Type, Color> _typeColors = new();

            // Constants for UI layout
            private const float ColorBarWidth = 6f;
            private const float DragHandleWidth = 20f;

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

            private void OnEnable()
            {
                  // Initialize the target asset and serialized properties
                  _targetAsset = (ScriptableAsset)target;

                  if (!_targetAsset)
                  {
                        Debug.LogError("[ScriptableEditor] Target asset is null. Ensure the scriptable object is assigned correctly.");

                        return;
                  }

                  // Find the serialized property for the asset data
                  _allDataProperty = serializedObject.FindProperty("assetData");

                  // Check if the property is found and is an array/list
                  if (_allDataProperty is not { isArray: true })
                  {
                        Debug.LogError("[ScriptableEditor] SerializedProperty 'assetData' not found or is not a list/array.");

                        return;
                  }

                  // Initialize editor styles and reorderable list
                  InitializeDataTypeReflectionAndColors();
                  SetupReorderableList();
                  ResetPendingData();
            }

            public override void OnInspectorGUI()
            {
                  // Ensure the target asset is valid
                  InitializeEditorStyles();
                  serializedObject.Update();

                  if (Event.current.type == EventType.Layout)
                  {
                        ValidateAllNames();
                  }

                  if (_reorderableList != null)
                  {
                        _reorderableList.DoLayoutList();
                  }
                  else
                  {
                        EditorGUILayout.HelpBox("Data list could not be initialized.", MessageType.Warning);

                        if (_allDataProperty != null)
                        {
                              EditorGUILayout.PropertyField(_allDataProperty, true);
                        }
                        else
                        {
                              DrawDefaultInspector();
                        }
                  }

                  DrawAddDataSection();

                  if (serializedObject.ApplyModifiedProperties())
                  {
                        ValidateAllNames();
                  }
            }
      }
}