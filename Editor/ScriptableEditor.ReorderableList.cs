using System;
using ScriptableAsset.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private void SetupReorderableList()
            {
                  if (_allDataProperty == null)
                  {
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
                                    float currentX = rect.x;
                                    float searchWidth = Mathf.Max(rect.width * 0.4f, 150f);
                                    float buttonWidth = Mathf.Max(rect.width * 0.15f, 80f);
                                    float remainingWidth = rect.width - searchWidth - (buttonWidth * 3) - 15;

                                    EditorGUI.LabelField(new Rect(currentX, rect.y, Mathf.Max(0, remainingWidth), EditorGUIUtility.singleLineHeight), "Data Objects");
                                    currentX += Mathf.Max(0, remainingWidth) + 5;

                                    _searchText = EditorGUI.TextField(new Rect(currentX, rect.y, searchWidth, EditorGUIUtility.singleLineHeight),
                                                _searchText,
                                                EditorStyles.toolbarSearchField);
                                    currentX += searchWidth + 5;

                                    if (GUI.Button(new Rect(currentX, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "Name ▼", EditorStyles.toolbarButton))
                                    {
                                          ApplySort(SortMode.ByNameAsc);
                                    }

                                    currentX += buttonWidth;

                                    if (GUI.Button(new Rect(currentX, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "Name ▲", EditorStyles.toolbarButton))
                                    {
                                          ApplySort(SortMode.ByNameDesc);
                                    }

                                    currentX += buttonWidth;

                                    if (GUI.Button(new Rect(currentX, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "Type", EditorStyles.toolbarButton))
                                    {
                                          ApplySort(SortMode.ByType);
                                    }
                              }
                  };

                  _reorderableList.drawElementCallback = (rect, index, _, _) =>
                  {
                        if (!_stylesInitialized)
                        {
                              InitializeEditorStyles();
                        }

                        SerializedProperty element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);

                        if (element.managedReferenceValue is not DataObject dataObject)
                        {
                              EditorGUI.LabelField(rect, "Invalid data element.");

                              return;
                        }

                        bool isDimmed = !string.IsNullOrEmpty(_searchText) &&
                                        !dataObject.name.ToLowerInvariant().Contains(_searchText.ToLowerInvariant(), StringComparison.Ordinal);

                        var blockRect = new Rect(rect.x + DragHandleWidth, rect.y + 1, rect.width - DragHandleWidth - 1, rect.height - 2);
                        GUI.Box(blockRect, GUIContent.none, _blockStyle);

                        var contentRect = new Rect(blockRect.x + _blockStyle.padding.left,
                                    blockRect.y + _blockStyle.padding.top,
                                    blockRect.width - _blockStyle.padding.horizontal,
                                    blockRect.height - _blockStyle.padding.vertical);

                        if (isDimmed)
                        {
                              EditorGUI.BeginDisabledGroup(true);
                        }

                        var colorBarRect = new Rect(contentRect.x, contentRect.y, ColorBarWidth, contentRect.height);

                        if (_typeColors.TryGetValue(dataObject.GetType(), out Color typeColor))
                        {
                              EditorGUI.DrawRect(colorBarRect, typeColor);
                        }

                        var fieldsRect = new Rect(contentRect.x + ColorBarWidth + 4, contentRect.y, contentRect.width - ColorBarWidth - 4, contentRect.height);

                        var nameLineRect = new Rect(fieldsRect.x, fieldsRect.y, fieldsRect.width, EditorGUIUtility.singleLineHeight);
                        float nameFieldWidth = nameLineRect.width * 0.65f;
                        float typeFieldWidth = nameLineRect.width - nameFieldWidth - 5;

                        SerializedProperty nameProperty = element.FindPropertyRelative("name");

                        if (nameProperty != null)
                        {
                              EditorGUI.BeginChangeCheck();
                              EditorGUI.PropertyField(new Rect(nameLineRect.x, nameLineRect.y, nameFieldWidth, nameLineRect.height), nameProperty, GUIContent.none);

                              if (EditorGUI.EndChangeCheck())
                              {
                                    ValidateAllNames();
                              }
                        }
                        else
                        {
                              EditorGUI.LabelField(new Rect(nameLineRect.x, nameLineRect.y, nameFieldWidth, nameLineRect.height), dataObject.name);
                        }

                        EditorGUI.LabelField(new Rect(nameLineRect.x + nameFieldWidth + 5, nameLineRect.y, typeFieldWidth, nameLineRect.height),
                                    dataObject.GetType().Name,
                                    _elementTypeLabelStyle);

                        float currentY = nameLineRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                        if (_isNameDuplicate.TryGetValue(index, out bool isDuplicate) && isDuplicate)
                        {
                              var errorRect = new Rect(fieldsRect.x, currentY, fieldsRect.width, EditorGUIUtility.singleLineHeight);
                              EditorGUI.LabelField(errorRect, "This name is already in use.", _errorLabelStyle);
                              currentY += errorRect.height;
                        }


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

                              if (currentProp.propertyPath.EndsWith(".value", StringComparison.Ordinal))
                              {
                                    float originalLabelWidth = EditorGUIUtility.labelWidth;
                                    EditorGUIUtility.labelWidth = 50f;
                                    EditorGUI.PropertyField(propRect, currentProp, true);
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

                        if (_isNameDuplicate.TryGetValue(index, out bool isDuplicate) && isDuplicate)
                        {
                              height += EditorGUIUtility.singleLineHeight;
                        }

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