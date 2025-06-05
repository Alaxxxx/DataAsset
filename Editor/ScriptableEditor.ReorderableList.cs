using System;
using System.Collections.Generic;
using ScriptableAsset.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private void SetupReorderableList()
            {
                  if (_allDataProperty == null)
                  {
                        Debug.LogError("[ScriptableEditor.SetupReorderableList] _allDataProperty is null. Cannot setup list.");

                        return;
                  }

                  _reorderableList = new ReorderableList(serializedObject,
                              _allDataProperty,
                              draggable: true,
                              displayHeader: true,
                              displayAddButton: false,
                              displayRemoveButton: true)
                  {
                              drawHeaderCallback = rect =>
                              {
                                    float totalWidth = rect.width;
                                    float currentX = rect.x;
                                    const float spacing = 5f;

                                    var titleContent = new GUIContent("Data Objects");
                                    float titleWidth = EditorStyles.label.CalcSize(titleContent).x;
                                    EditorGUI.LabelField(new Rect(currentX, rect.y, titleWidth, EditorGUIUtility.singleLineHeight), titleContent);
                                    currentX += titleWidth + spacing * 2;

                                    float searchWidth = Mathf.Max(totalWidth * 0.25f, 100f);

                                    _searchText = EditorGUI.TextField(new Rect(currentX, rect.y, searchWidth, EditorGUIUtility.singleLineHeight),
                                                _searchText,
                                                EditorStyles.toolbarSearchField);
                                    currentX += searchWidth + spacing * 2;

                                    const float sortButtonWidth = 70f;
                                    const float scanButtonWidth = 90f;
                                    const float buttonsAreaWidth = (sortButtonWidth * 3) + (spacing * 2) + scanButtonWidth;
                                    float flexibleSpace = totalWidth - currentX - buttonsAreaWidth;

                                    if (flexibleSpace > 0)
                                    {
                                          currentX += flexibleSpace;
                                    }

                                    if (GUI.Button(new Rect(currentX, rect.y, sortButtonWidth, EditorGUIUtility.singleLineHeight), "Name ▼", EditorStyles.toolbarButton))
                                    {
                                          ApplySort(SortMode.ByNameAsc);
                                    }

                                    currentX += sortButtonWidth;

                                    if (GUI.Button(new Rect(currentX, rect.y, sortButtonWidth, EditorGUIUtility.singleLineHeight), "Name ▲", EditorStyles.toolbarButton))
                                    {
                                          ApplySort(SortMode.ByNameDesc);
                                    }

                                    currentX += sortButtonWidth;

                                    if (GUI.Button(new Rect(currentX, rect.y, sortButtonWidth, EditorGUIUtility.singleLineHeight), "Type", EditorStyles.toolbarButton))
                                    {
                                          ApplySort(SortMode.ByType);
                                    }

                                    currentX += sortButtonWidth + spacing;

                                    GUIContent scanButtonContent = _isScanningUsages
                                                ? new GUIContent("Scanning...", "Scanning project for data usages")
                                                : new GUIContent("Scan Usages", "Scan scripts potentially referencing this asset's data names");
                                    EditorGUI.BeginDisabledGroup(_isScanningUsages);

                                    if (GUI.Button(new Rect(currentX, rect.y, scanButtonWidth, EditorGUIUtility.singleLineHeight), scanButtonContent, EditorStyles.toolbarButton))
                                    {
                                          StartScanForDataUsages();
                                    }

                                    EditorGUI.EndDisabledGroup();
                              }
                  };

                  _reorderableList.drawElementCallback =
                              (rect, index, _, _) =>
                              {
                                    if (!_stylesInitialized)
                                    {
                                          InitializeEditorStyles();
                                    }

                                    SerializedProperty element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                                    var dataObject = element.managedReferenceValue as DataObject;

                                    if (dataObject == null)
                                    {
                                          EditorGUI.LabelField(rect, "Invalid data element (null reference).");

                                          return;
                                    }

                                    bool isDimmed = !string.IsNullOrEmpty(_searchText) &&
                                                    !dataObject.name.ToLowerInvariant().Contains(_searchText.ToLowerInvariant(), StringComparison.Ordinal);

                                    var elementBackgroundRect = new Rect(rect.x + DragHandleWidth, rect.y + 1, rect.width - DragHandleWidth - 1, rect.height - 2);
                                    GUI.Box(elementBackgroundRect, GUIContent.none, _blockStyle);

                                    var contentRect = new Rect(elementBackgroundRect.x + _blockStyle.padding.left,
                                                elementBackgroundRect.y + _blockStyle.padding.top,
                                                elementBackgroundRect.width - _blockStyle.padding.horizontal,
                                                elementBackgroundRect.height - _blockStyle.padding.vertical);

                                    if (isDimmed)
                                    {
                                          EditorGUI.BeginDisabledGroup(true);
                                    }

                                    var colorBarRect = new Rect(contentRect.x, contentRect.y, ColorBarWidth, contentRect.height);

                                    if (_typeColors.TryGetValue(dataObject.GetType(), out Color typeColor))
                                    {
                                          EditorGUI.DrawRect(colorBarRect, typeColor);
                                    }

                                    var fieldsRect = new Rect(contentRect.x + ColorBarWidth + 4,
                                                contentRect.y,
                                                contentRect.width - ColorBarWidth - 4,
                                                contentRect.height);
                                    var nameLineRect = new Rect(fieldsRect.x, fieldsRect.y, fieldsRect.width, EditorGUIUtility.singleLineHeight);

                                    string usageFoldoutLabel = "";
                                    float usageFoldoutWidth = 0f;
                                    List<UsageInfo> usages = null;
                                    bool hasUsages = _detailedDataUsages != null && _detailedDataUsages.TryGetValue(dataObject.name, out usages) && usages.Count > 0;

                                    if (hasUsages)
                                    {
                                          usageFoldoutLabel = $" ({usages.Count} refs)";
                                          usageFoldoutWidth = EditorStyles.foldout.CalcSize(new GUIContent(usageFoldoutLabel)).x + 5f;
                                    }

                                    float typeNameDisplayWidth = EditorStyles.miniLabel.CalcSize(new GUIContent(dataObject.GetType().Name)).x +
                                                                 _elementTypeLabelStyle.padding.horizontal + 5f;
                                    float nameLabelWidth = 40f;
                                    float nameFieldAvailableWidth = nameLineRect.width - nameLabelWidth - usageFoldoutWidth - typeNameDisplayWidth - 5f;
                                    float nameFieldActualWidth = Mathf.Max(nameFieldAvailableWidth, 50f);

                                    EditorGUI.LabelField(new Rect(nameLineRect.x, nameLineRect.y, nameLabelWidth, nameLineRect.height), "Name:");
                                    SerializedProperty nameProperty = element.FindPropertyRelative("name");

                                    if (nameProperty != null)
                                    {
                                          EditorGUI.BeginChangeCheck();

                                          EditorGUI.PropertyField(new Rect(nameLineRect.x + nameLabelWidth, nameLineRect.y, nameFieldActualWidth, nameLineRect.height),
                                                      nameProperty,
                                                      GUIContent.none);

                                          if (EditorGUI.EndChangeCheck())
                                          {
                                                ValidateAllNames();
                                          }
                                    }
                                    else
                                    {
                                          EditorGUI.LabelField(new Rect(nameLineRect.x + nameLabelWidth, nameLineRect.y, nameFieldActualWidth, nameLineRect.height),
                                                      dataObject.name);
                                    }

                                    float currentXOnNameLine = nameLineRect.x + nameLabelWidth + nameFieldActualWidth;

                                    if (hasUsages)
                                    {
                                          _foldoutUsageStates.TryAdd(index, false);

                                          _foldoutUsageStates[index] =
                                                      EditorGUI.Foldout(new Rect(currentXOnNameLine, nameLineRect.y, usageFoldoutWidth, nameLineRect.height),
                                                                  _foldoutUsageStates[index],
                                                                  usageFoldoutLabel,
                                                                  true,
                                                                  EditorStyles.foldout);
                                          currentXOnNameLine += usageFoldoutWidth;
                                    }

                                    EditorGUI.LabelField(new Rect(currentXOnNameLine + 5f, nameLineRect.y, typeNameDisplayWidth, nameLineRect.height),
                                                dataObject.GetType().Name,
                                                _elementTypeLabelStyle);

                                    float currentY = nameLineRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                                    if (hasUsages && _foldoutUsageStates.TryGetValue(index, out bool isUsageFoldoutExpanded) && isUsageFoldoutExpanded)
                                    {
                                          EditorGUI.indentLevel++;

                                          foreach (UsageInfo usage in usages)
                                          {
                                                var usageDetailRect = new Rect(fieldsRect.x, currentY, fieldsRect.width, EditorGUIUtility.singleLineHeight);
                                                string goNamePart = string.IsNullOrEmpty(usage.GameObjectName) ? "" : $"on GO '{usage.GameObjectName}' ";
                                                string displayText = $"↳ in '{usage.ScriptName}' {goNamePart}({usage.ContainerType}: {usage.ContainerName})";
                                                var usageContent = new GUIContent(displayText, $"{usage.ScriptPath}\n(Container: {usage.ContainerPath})");

                                                if (GUI.Button(usageDetailRect, usageContent, EditorStyles.label))
                                                {
                                                      var scriptObj = AssetDatabase.LoadAssetAtPath<Object>(usage.ScriptPath);

                                                      if (scriptObj)
                                                      {
                                                            EditorGUIUtility.PingObject(scriptObj);
                                                      }
                                                }

                                                currentY += usageDetailRect.height + EditorGUIUtility.standardVerticalSpacing / 2;
                                          }

                                          EditorGUI.indentLevel--;
                                          currentY += EditorGUIUtility.standardVerticalSpacing / 2;
                                    }

                                    if (_isNameDuplicate.TryGetValue(index, out bool isDuplicate) && isDuplicate)
                                    {
                                          var errorRect = new Rect(fieldsRect.x, currentY, fieldsRect.width, EditorGUIUtility.singleLineHeight);
                                          EditorGUI.LabelField(errorRect, "This name is already in use.", _errorLabelStyle);
                                          currentY += errorRect.height + EditorGUIUtility.standardVerticalSpacing;
                                    }

                                    currentY += 2f;

                                    SerializedProperty currentProp = element.Copy();
                                    bool enterChildren = true;

                                    while (currentProp.NextVisible(enterChildren))
                                    {
                                          enterChildren = false;

                                          if (SerializedProperty.EqualContents(currentProp, element.GetEndProperty()))
                                          {
                                                break;
                                          }

                                          if (currentProp.name == "name")
                                          {
                                                continue;
                                          }

                                          var propRect = new Rect(fieldsRect.x, currentY, fieldsRect.width, EditorGUI.GetPropertyHeight(currentProp, true));

                                          if (currentProp.propertyPath.EndsWith(".value", StringComparison.OrdinalIgnoreCase))
                                          {
                                                float originalLabelWidth = EditorGUIUtility.labelWidth;
                                                EditorGUIUtility.labelWidth = 40f;

                                                EditorGUI.LabelField(new Rect(propRect.x, propRect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                                                            new GUIContent(currentProp.displayName));

                                                EditorGUI.PropertyField(new Rect(propRect.x + EditorGUIUtility.labelWidth,
                                                                        propRect.y,
                                                                        propRect.width - EditorGUIUtility.labelWidth,
                                                                        propRect.height),
                                                            currentProp,
                                                            GUIContent.none,
                                                            true);
                                                EditorGUIUtility.labelWidth = originalLabelWidth;
                                          }
                                          else
                                          {
                                                EditorGUI.PropertyField(propRect, currentProp, true);
                                          }

                                          currentY += propRect.height + EditorGUIUtility.standardVerticalSpacing;
                                    }

                                    if (isDimmed)
                                    {
                                          EditorGUI.EndDisabledGroup();
                                    }
                              };

                  _reorderableList.elementHeightCallback = index =>
                  {
                        if (!_stylesInitialized)
                        {
                              InitializeEditorStyles();
                        }

                        SerializedProperty element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                        float height = EditorGUIUtility.singleLineHeight;
                        height += _blockStyle.padding.vertical;
                        height += 4;
                        height += EditorGUIUtility.standardVerticalSpacing * 2;

                        if (_isNameDuplicate.TryGetValue(index, out bool isDuplicate) && isDuplicate)
                        {
                              height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        }

                        List<UsageInfo> usages = null;

                        bool hasUsages = _detailedDataUsages != null &&
                                         _detailedDataUsages.TryGetValue((element.managedReferenceValue as DataObject)?.name ?? "", out usages) && usages.Count > 0;

                        if (hasUsages && _foldoutUsageStates.TryGetValue(index, out bool isUsageFoldoutExpanded) && isUsageFoldoutExpanded)
                        {
                              height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing / 2) * usages.Count;
                              height += EditorGUIUtility.standardVerticalSpacing / 2;
                        }

                        height += 2f;

                        SerializedProperty currentProp = element.Copy();
                        bool enterChildren = true;

                        while (currentProp.NextVisible(enterChildren))
                        {
                              enterChildren = false;

                              if (SerializedProperty.EqualContents(currentProp, element.GetEndProperty()))
                              {
                                    break;
                              }

                              if (currentProp.name == "name")
                              {
                                    continue;
                              }

                              height += EditorGUI.GetPropertyHeight(currentProp, true) + EditorGUIUtility.standardVerticalSpacing;
                        }

                        return Mathf.Max(height, EditorGUIUtility.singleLineHeight * 2 + _blockStyle.padding.vertical + 4);
                  };

                  _reorderableList.onRemoveCallback = l =>
                  {
                        SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(l.index);

                        if (element.managedReferenceValue != null)
                        {
                              element.managedReferenceValue = null;
                        }

                        l.serializedProperty.DeleteArrayElementAtIndex(l.index);

                        if (l.index >= l.serializedProperty.arraySize - 1 && l.serializedProperty.arraySize > 0)
                        {
                              l.index = l.serializedProperty.arraySize - 1;
                        }

                        ValidateAllNames();
                        EditorUtility.SetDirty(_targetAsset);
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                  };
            }
      }
}