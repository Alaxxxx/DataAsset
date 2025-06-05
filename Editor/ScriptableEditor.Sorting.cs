using System;
using System.Collections.Generic;
using ScriptableAsset.Core;
using UnityEditor;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private enum SortMode
            {
                  None,
                  ByNameAsc,
                  ByNameDesc,
                  ByType
            }

            private void ApplySort(SortMode mode)
            {
                  if (_allDataProperty is not { arraySize: > 1 })
                  {
                        return;
                  }

                  Undo.RecordObject(_targetAsset, "Sort Data Objects");

                  var tempList = new List<DataObject>(_allDataProperty.arraySize);

                  for (int i = 0; i < _allDataProperty.arraySize; i++)
                  {
                        tempList.Add(_allDataProperty.GetArrayElementAtIndex(i).managedReferenceValue as DataObject);
                  }

                  switch (mode)
                  {
                        case SortMode.ByNameAsc:
                              tempList.Sort(static (a, b) => string.Compare(a?.name, b?.name, StringComparison.OrdinalIgnoreCase));

                              break;
                        case SortMode.ByNameDesc:
                              tempList.Sort(static (a, b) => string.Compare(b?.name, a?.name, StringComparison.OrdinalIgnoreCase));

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
                  EditorUtility.SetDirty(_targetAsset);
                  serializedObject.ApplyModifiedProperties();
                  serializedObject.Update();
            }
      }
}