using UnityEditor;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private GUIStyle _elementTypeLabelStyle;
            private GUIStyle _blockStyle;
            private GUIStyle _errorLabelStyle;

            private void InitializeEditorStyles()
            {
                  if (_stylesInitialized)
                  {
                        return;
                  }

                  _elementTypeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                  {
                              fontStyle = FontStyle.Italic,
                              alignment = TextAnchor.MiddleRight,
                              padding = new RectOffset(0, 5, 0, 0)
                  };

                  _blockStyle = new GUIStyle(GUI.skin.box)
                  {
                              padding = new RectOffset(8, 8, 8, 8),
                              margin = new RectOffset(0, 0, 2, 2)
                  };

                  _errorLabelStyle = new GUIStyle(EditorStyles.label)
                  {
                              normal = { textColor = Color.red },
                              fontSize = 9,
                              padding = new RectOffset(0, 0, 0, 0)
                  };
                  _stylesInitialized = true;
            }
      }
}