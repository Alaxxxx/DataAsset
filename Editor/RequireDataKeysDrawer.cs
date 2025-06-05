using System.Collections.Generic;
using System.Linq;
using ScriptableAsset.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      [CustomPropertyDrawer(typeof(RequireDataKeysAttribute))]
      public class RequireDataKeysDrawer : PropertyDrawer
      {
            private const float WarningBoxSpacing = 2f;
            private const float HelpBoxAdditionalVerticalPadding = 10f;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                  var propertyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                  EditorGUI.PropertyField(propertyRect, property, label, true);

                  if (!property.objectReferenceValue)
                  {
                        return;
                  }

                  var soAsset = property.objectReferenceValue as Core.ScriptableAsset;
                  string message = null;
                  var messageType = MessageType.None;

                  if (soAsset)
                  {
                        var assetAttribute = (RequireDataKeysAttribute)attribute;
                        List<string> missingKeys = GetMissingKeys(soAsset, assetAttribute.RequiredKeys);

                        if (missingKeys.Any())
                        {
                              message = $"Missing expected data key(s) in '{soAsset.name}': {string.Join(", ", missingKeys)}. Check asset configuration.";
                              messageType = MessageType.Warning;
                        }
                  }
                  else
                  {
                        message = $"Assigned object is not a '{nameof(Core.ScriptableAsset)}'. Expected type: {typeof(Core.ScriptableAsset).FullName}";
                        messageType = MessageType.Error;
                  }

                  if (message != null)
                  {
                        float helpBoxHeight = CalculatePaddedHelpBoxHeight(message, position.width);
                        var helpBoxRect = new Rect(position.x, propertyRect.yMax + WarningBoxSpacing, position.width, helpBoxHeight);
                        EditorGUI.HelpBox(helpBoxRect, message, messageType);
                  }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                  float height = EditorGUIUtility.singleLineHeight;

                  if (property.objectReferenceValue)
                  {
                        string messageContentForHeight = null;

                        if (property.objectReferenceValue is Core.ScriptableAsset soAsset)
                        {
                              var keysAttribute = (RequireDataKeysAttribute)this.attribute;
                              List<string> missingKeys = GetMissingKeys(soAsset, keysAttribute.RequiredKeys);

                              if (missingKeys.Any())
                              {
                                    messageContentForHeight = $"Missing keys: {string.Join(", ", missingKeys)}";
                              }
                        }
                        else
                        {
                              messageContentForHeight = $"Assigned object is not a '{nameof(Core.ScriptableAsset)}'.";
                        }

                        if (messageContentForHeight != null)
                        {
                              height += CalculatePaddedHelpBoxHeight(messageContentForHeight, EditorGUIUtility.currentViewWidth) + WarningBoxSpacing;
                        }
                  }

                  return height;
            }

            private static List<string> GetMissingKeys(Core.ScriptableAsset asset, string[] requiredKeys)
            {
                  var missingKeys = new List<string>();

                  if (!asset || requiredKeys == null)
                  {
                        return missingKeys;
                  }

                  foreach (string key in requiredKeys)
                  {
                        if (string.IsNullOrEmpty(key))
                        {
                              continue;
                        }

                        if (asset.GetData(key) == null)
                        {
                              missingKeys.Add($"'{key}'");
                        }
                  }

                  return missingKeys;
            }

            private static float CalculatePaddedHelpBoxHeight(string message, float availableWidth)
            {
                  if (string.IsNullOrEmpty(message))
                  {
                        return 0f;
                  }

                  float textHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(message), availableWidth);

                  return textHeight + HelpBoxAdditionalVerticalPadding;
            }
      }
}