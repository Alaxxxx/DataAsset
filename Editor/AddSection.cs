using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableAsset.Core;
using UnityEditor;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private void DrawAddDataSectionLayout()
            {
                  EditorGUILayout.Space(SectionSeparatorSpace);
                  Rect separatorRect = EditorGUILayout.GetControlRect(false, 1);
                  separatorRect.x -= 2;
                  separatorRect.width += 4;
                  EditorGUI.DrawRect(separatorRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                  EditorGUILayout.Space(AddSectionTopSpace);

                  EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                  EditorGUILayout.LabelField("Add New Data", EditorStyles.boldLabel);
                  EditorGUILayout.Space(5);

                  if (_dataTypes == null || _dataTypes.Length == 0)
                  {
                        EditorGUILayout.HelpBox("No compatible data types found (inheriting from DataObject).", MessageType.Info);
                  }
                  else
                  {
                        if (_pendingType == null)
                        {
                              int selectedTypeIndex = EditorGUILayout.Popup(new GUIContent("Data Type"),
                                          -1,
                                          _dataTypeDisplayNames.Select(static n => new GUIContent(n)).ToArray());

                              if (selectedTypeIndex != -1)
                              {
                                    _pendingType = _dataTypes[selectedTypeIndex];
                                    string baseName = $"New {_pendingType.Name}";
                                    string potentialName = baseName;
                                    int counter = 1;
                                    var existingNames = new List<string>();

                                    if (_allDataProperty != null)
                                    {
                                          for (int i = 0; i < _allDataProperty.arraySize; ++i)
                                          {
                                                if (_allDataProperty.GetArrayElementAtIndex(i).managedReferenceValue is DataObject item)
                                                {
                                                      existingNames.Add(item.dataName);
                                                }
                                          }
                                    }

                                    while (existingNames.Contains(potentialName))
                                    {
                                          potentialName = $"{baseName} {counter++}";
                                    }

                                    _pendingName = potentialName;

                                    _pendingIntValue = 0;
                                    _pendingStringValue = "";
                                    _pendingBoolValue = false;
                                    _pendingByteValue = 0;
                                    _pendingShortValue = 0;
                                    _pendingFloatValue = 0;
                                    _pendingLongValue = 0;
                              }
                        }
                        else
                        {
                              EditorGUILayout.LabelField($"Configure: {_pendingType.Name}", EditorStyles.miniBoldLabel);
                              EditorGUILayout.Space(2);
                              _pendingName = EditorGUILayout.TextField("Name", _pendingName);
                              EditorGUILayout.Space(2);

                              bool isBaseTypeHandledForConfig = false;

                              if (typeof(ReactiveValue<int>).IsAssignableFrom(_pendingType))
                              {
                                    _pendingIntValue = EditorGUILayout.IntField("Initial Value", _pendingIntValue);
                                    isBaseTypeHandledForConfig = true;
                              }
                              else if (typeof(ReactiveValue<string>).IsAssignableFrom(_pendingType))
                              {
                                    _pendingStringValue = EditorGUILayout.TextField("Initial Value", _pendingStringValue ?? "");
                                    isBaseTypeHandledForConfig = true;
                              }
                              else if (typeof(ReactiveValue<bool>).IsAssignableFrom(_pendingType))
                              {
                                    _pendingBoolValue = EditorGUILayout.Toggle("Initial Value", _pendingBoolValue);
                                    isBaseTypeHandledForConfig = true;
                              }
                              else if (typeof(ReactiveValue<byte>).IsAssignableFrom(_pendingType))
                              {
                                    int val = EditorGUILayout.IntField("Initial Value", _pendingByteValue);

                                    if (val < byte.MinValue)
                                    {
                                          val = byte.MinValue;
                                    }

                                    if (val > byte.MaxValue)
                                    {
                                          val = byte.MaxValue;
                                    }

                                    _pendingByteValue = (byte)val;
                                    isBaseTypeHandledForConfig = true;
                              }
                              else if (typeof(ReactiveValue<short>).IsAssignableFrom(_pendingType))
                              {
                                    int val = EditorGUILayout.IntField("Initial Value", _pendingShortValue);

                                    if (val < short.MinValue)
                                    {
                                          val = short.MinValue;
                                    }

                                    if (val > short.MaxValue)
                                    {
                                          val = short.MaxValue;
                                    }

                                    _pendingShortValue = (short)val;
                                    isBaseTypeHandledForConfig = true;
                              }
                              else if (typeof(ReactiveValue<float>).IsAssignableFrom(_pendingType))
                              {
                                    _pendingFloatValue = EditorGUILayout.FloatField("Initial Value", _pendingFloatValue);
                                    isBaseTypeHandledForConfig = true;
                              }
                              else if (typeof(ReactiveValue<long>).IsAssignableFrom(_pendingType))
                              {
                                    _pendingLongValue = EditorGUILayout.LongField("Initial Value", _pendingLongValue);
                                    isBaseTypeHandledForConfig = true;
                              }

                              if (!isBaseTypeHandledForConfig && typeof(DataObject).IsAssignableFrom(_pendingType))
                              {
                                    EditorGUILayout.HelpBox($"'{_pendingType.Name}' will be added with default values.", MessageType.Info);
                              }

                              EditorGUILayout.Space(5);
                              EditorGUILayout.BeginHorizontal();

                              if (GUILayout.Button("Add"))
                              {
                                    try
                                    {
                                          var newDataInstance = (DataObject)Activator.CreateInstance(_pendingType);
                                          newDataInstance.dataName = _pendingName;

                                          switch (newDataInstance)
                                          {
                                                case ReactiveValue<int> rInt:
                                                      rInt.Value = _pendingIntValue;

                                                      break;
                                                case ReactiveValue<string> rString:
                                                      rString.Value = _pendingStringValue;

                                                      break;
                                                case ReactiveValue<bool> rBool:
                                                      rBool.Value = _pendingBoolValue;

                                                      break;
                                                case ReactiveValue<byte> rByte:
                                                      rByte.Value = _pendingByteValue;

                                                      break;
                                                case ReactiveValue<short> rShort:
                                                      rShort.Value = _pendingShortValue;

                                                      break;
                                                case ReactiveValue<float> rFloat:
                                                      rFloat.Value = _pendingFloatValue;

                                                      break;
                                                case ReactiveValue<long> rLong:
                                                      rLong.Value = _pendingLongValue;

                                                      break;
                                          }

                                          int newElementIndex = _allDataProperty.arraySize;
                                          _allDataProperty.InsertArrayElementAtIndex(newElementIndex);
                                          SerializedProperty newElementProperty = _allDataProperty.GetArrayElementAtIndex(newElementIndex);
                                          newElementProperty.managedReferenceValue = newDataInstance;

                                          ValidateAllNames();

                                          if (_reorderableList != null)
                                          {
                                                _reorderableList.index = newElementIndex;
                                          }

                                          ResetPendingData();
                                          EditorUtility.SetDirty(_targetAsset);
                                    }
                                    catch (Exception ex)
                                    {
                                          Debug.LogError($"[ScriptableEditor] Error during instance creation or addition: {ex}");
                                    }
                              }

                              if (GUILayout.Button("Cancel"))
                              {
                                    ResetPendingData();
                              }

                              EditorGUILayout.EndHorizontal();
                        }
                  }

                  EditorGUILayout.EndVertical();
            }

            private void ResetPendingData()
            {
                  _pendingType = null;
                  _pendingName = "";
                  _pendingIntValue = 0;
                  _pendingStringValue = "";
                  _pendingBoolValue = false;
                  _pendingByteValue = 0;
                  _pendingShortValue = 0;
                  _pendingFloatValue = 0f;
                  _pendingLongValue = 0L;
            }
      }
}