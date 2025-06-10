using System.Collections.Generic;
using System.Linq;
using DataAsset.Core;
using UnityEditor;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private readonly Dictionary<int, bool> _isNameDuplicate = new();

            /// <summary>
            /// Validates the uniqueness of all object names in the serialized data property.
            /// Any duplicate names within the data will be marked in the `_isNameDuplicate` dictionary.
            /// </summary>
            /// <remarks>
            /// - This method iterates through a serialized array property (`_allDataProperty`) to extract and validate names.
            /// - If a dataName is found to be non-unique across the collection, its index is flagged as duplicate in the `_isNameDuplicate` dictionary.
            /// - Empty or null names are ignored during the validation process.
            /// - The method will return early if `_allDataProperty` is null.
            /// </remarks>
            /// <seealso cref="ScriptableEditor.ApplySort"/>
            /// <seealso cref="ScriptableEditor.OnInspectorGUI"/>
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
                        names.Add(element.managedReferenceValue is DataObject dataObject ? dataObject.dataName : null);
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