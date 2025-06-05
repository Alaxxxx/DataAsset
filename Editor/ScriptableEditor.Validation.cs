using System.Collections.Generic;
using System.Linq;
using ScriptableAsset.Core;
using UnityEditor;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private readonly Dictionary<int, bool> _isNameDuplicate = new();

            private void ValidateAllNames()
            {
                  _isNameDuplicate.Clear();

                  if (_allDataProperty == null)
                  {
                        return;
                  }

                  var names = new List<string>();

                  for (int i = 0; i < _allDataProperty.arraySize; i++)
                  {
                        SerializedProperty element = _allDataProperty.GetArrayElementAtIndex(i);
                        names.Add(element.managedReferenceValue is DataObject dataObject ? dataObject.name : null);
                  }

                  for (int i = 0; i < names.Count; i++)
                  {
                        if (string.IsNullOrEmpty(names[i]))
                        {
                              continue;
                        }

                        int count = names.Count(t => names[i] == t);

                        if (count > 1)
                        {
                              _isNameDuplicate[i] = true;
                        }
                  }
            }
      }
}