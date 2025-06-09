using System.Collections.Generic;
using ScriptableAsset.Core;
using ScriptableAsset.Core.Struct;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableAsset.Editor
{
      public partial class ScriptableEditor
      {
            private void DrawSingleDataObjectInspector()
            {
                  SerializedProperty singleElementProperty = _allDataProperty.GetArrayElementAtIndex(0);

                  if (singleElementProperty.managedReferenceValue is not DataObject dataObject)
                  {
                        EditorGUILayout.HelpBox("The single DataObject is null or invalid. You can clear it or add a new one.", MessageType.Error);

                        DrawAddDataSectionLayout();
                        EditorGUILayout.Space();
                        DrawClearSingleDataObjectButton();

                        return;
                  }

                  EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                  EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                  if (_typeColors.TryGetValue(dataObject.GetType(), out Color typeColor))
                  {
                        Rect colorStripRect = GUILayoutUtility.GetRect(ElementColorBarWidth + 2, EditorGUIUtility.singleLineHeight, GUILayout.ExpandHeight(true));

                        EditorGUI.DrawRect(new Rect(colorStripRect.x + 2, colorStripRect.y + 2, ElementColorBarWidth, colorStripRect.height - 4), typeColor);
                  }

                  GUILayout.Space(ElementColorBarWidth + 4);

                  string headerTitle = $"{_targetAsset.name}: {dataObject.dataName} ({dataObject.GetType().Name})";
                  GUILayout.Label(new GUIContent(headerTitle, "Currently editing this single data object."), EditorStyles.boldLabel);
                  GUILayout.FlexibleSpace();

                  GUIContent scanButtonContent = _isScanningUsages
                              ? new GUIContent("Scanning...", "Scanning for usages of " + dataObject.dataName)
                              : new GUIContent("Scan Usages", "Scan for usages of " + dataObject.dataName);
                  EditorGUI.BeginDisabledGroup(_isScanningUsages);

                  if (GUILayout.Button(scanButtonContent, EditorStyles.toolbarButton, GUILayout.Width(HeaderScanButtonWidth)))
                  {
                        StartScanForDataUsages();
                  }

                  EditorGUI.EndDisabledGroup();
                  EditorGUILayout.EndHorizontal();

                  EditorGUILayout.Space(SmallVerticalSpacing);

                  SerializedProperty nameProperty = singleElementProperty.FindPropertyRelative("dataName");

                  if (nameProperty != null)
                  {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(new GUIContent("Identifier Name", "The unique dataName to retrieve this data."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth - 5));
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(nameProperty, GUIContent.none);

                        if (EditorGUI.EndChangeCheck())
                        {
                              ValidateAllNames();
                        }

                        EditorGUILayout.EndHorizontal();
                  }

                  SerializedProperty currentProp = singleElementProperty.Copy();
                  bool enterChildren = true;
                  bool hasDrawnAnyProperty = false;

                  while (currentProp.NextVisible(enterChildren))
                  {
                        enterChildren = false;

                        if (SerializedProperty.EqualContents(currentProp, singleElementProperty.GetEndProperty()))
                        {
                              break;
                        }

                        if (currentProp.name == "dataName")
                        {
                              continue;
                        }

                        hasDrawnAnyProperty = true;
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(new GUIContent(currentProp.displayName, currentProp.tooltip), GUILayout.Width(ElementValueLabelWidth + 20));
                        EditorGUILayout.PropertyField(currentProp, GUIContent.none, true);
                        EditorGUILayout.EndHorizontal();
                  }

                  if (hasDrawnAnyProperty)
                  {
                        EditorGUILayout.Space(SmallVerticalSpacing);
                  }

                  List<UsageInfo> usages = null;
                  bool hasUsages = _detailedDataUsages != null && _detailedDataUsages.TryGetValue(dataObject.dataName, out usages) && usages.Count > 0;

                  if (hasUsages)
                  {
                        _foldoutUsageStates.TryAdd(0, false);

                        var foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader) { fontStyle = FontStyle.Bold };
                        _foldoutUsageStates[0] = EditorGUILayout.Foldout(_foldoutUsageStates[0], $"Usage References ({usages.Count})", true, foldoutStyle);

                        if (_foldoutUsageStates[0])
                        {
                              EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                              EditorGUI.indentLevel++;

                              foreach (UsageInfo usage in usages)
                              {
                                    string goNamePart = string.IsNullOrEmpty(usage.gameObjectName) ? "" : $"on GameObject '{usage.gameObjectName}' ";
                                    string displayText = $"↳ in '{usage.scriptName}' {goNamePart}({usage.containerType}: {usage.containerName})";
                                    var usageContent = new GUIContent(displayText, $"{usage.scriptPath}\n(Container: {usage.containerPath})");

                                    if (GUILayout.Button(usageContent, EditorStyles.label))
                                    {
                                          var scriptObj = AssetDatabase.LoadAssetAtPath<Object>(usage.scriptPath);

                                          if (scriptObj)
                                          {
                                                EditorGUIUtility.PingObject(scriptObj);
                                          }
                                    }
                              }

                              EditorGUI.indentLevel--;
                              EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.Space(SmallVerticalSpacing);
                  }

                  DrawClearSingleDataObjectButton();

                  EditorGUILayout.EndVertical();

                  EditorGUILayout.Space(SectionSeparatorSpace / 2);
                  Rect separatorRect = EditorGUILayout.GetControlRect(false, 1);
                  separatorRect.x -= 2;
                  separatorRect.width += 4;
                  EditorGUI.DrawRect(separatorRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                  EditorGUILayout.Space(AddSectionTopSpace / 2);

                  DrawAddDataSectionLayout();
            }

            private void DrawClearSingleDataObjectButton()
            {
                  EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                  EditorGUILayout.BeginHorizontal();
                  GUILayout.FlexibleSpace();
                  Color originalBgColor = GUI.backgroundColor;
                  GUI.backgroundColor = new Color(1f, 0.6f, 0.6f, 1f);

                  if (GUILayout.Button(new GUIContent(" Clear", EditorGUIUtility.IconContent("Toolbar Minus").image, "Remove this data object."),
                                  GUILayout.MaxWidth(150)))
                  {
                        _allDataProperty.ClearArray();
                        ValidateAllNames();
                        ResetPendingData();
                        _foldoutUsageStates.Clear();
                  }

                  GUI.backgroundColor = originalBgColor;
                  GUILayout.FlexibleSpace();
                  EditorGUILayout.EndHorizontal();
            }
      }
}