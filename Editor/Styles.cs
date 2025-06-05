using UnityEditor;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private GUIStyle _elementTypeLabelStyle;
            private GUIStyle _blockStyle;
            private GUIStyle _errorLabelStyle;

            /// <summary>
            /// Initializes custom GUI styles for the editor to enhance the visual representation
            /// and layout of the interface.
            /// This method ensures that styles are properly configured before being applied in the inspector.
            /// If the styles are already initialized, this method will simply return without reinitializing to avoid redundant processing.
            /// Configures the following custom styles:
            /// - `_elementTypeLabelStyle`: A right-aligned, italic style for element type labels.
            /// - `_blockStyle`: A padded box style for grouping elements.
            /// - `_errorLabelStyle`: A red-colored, compact label style for indicating errors.
            /// </summary>
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