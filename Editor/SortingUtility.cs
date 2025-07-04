﻿using System;
using System.Collections.Generic;
using DataAsset.Core;
using UnityEditor;

namespace DataAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            /// <summary>
            /// Specifies the modes available for sorting data objects within a <see cref="DataAsset.Editor.ScriptableEditor"/>.
            /// </summary>
            private enum SortMode
            {
                  None,
                  ByNameAsc,
                  ByNameDesc,
                  ByType
            }

            /// <summary>
            /// Sorts the data objects in the editor based on the specified sort mode.
            /// </summary>
            /// <param dataName="mode">The sorting mode to apply to the data objects.</param>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified sorting mode is not defined in <see cref="SortMode"/>.</exception>
            private void ApplySort(SortMode mode)
            {
                  if (_allDataProperty is not { arraySize: > 1 })
                  {
                        return;
                  }

                  Undo.RecordObject(_targetAssetSo, "Sort Data Objects");

                  var tempList = new List<DataObject>(_allDataProperty.arraySize);

                  for (int i = 0; i < _allDataProperty.arraySize; i++)
                  {
                        tempList.Add(_allDataProperty.GetArrayElementAtIndex(i).managedReferenceValue as DataObject);
                  }

                  switch (mode)
                  {
                        case SortMode.ByNameAsc:
                              tempList.Sort(static (a, b) => string.Compare(a?.dataName, b?.dataName, StringComparison.OrdinalIgnoreCase));

                              break;
                        case SortMode.ByNameDesc:
                              tempList.Sort(static (a, b) => string.Compare(b?.dataName, a?.dataName, StringComparison.OrdinalIgnoreCase));

                              break;
                        case SortMode.ByType:
                              tempList.Sort(static (a, b) => string.Compare(a?.GetType().Name, b?.GetType().Name, StringComparison.OrdinalIgnoreCase));

                              break;
                        case SortMode.None:
                              break;
                        default:
                              throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                  }

                  for (int i = 0; i < tempList.Count; i++)
                  {
                        _allDataProperty.GetArrayElementAtIndex(i).managedReferenceValue = tempList[i];
                  }

                  ValidateAllNames();
                  EditorUtility.SetDirty(_targetAssetSo);
                  serializedObject.ApplyModifiedProperties();
                  serializedObject.Update();
            }
      }
}