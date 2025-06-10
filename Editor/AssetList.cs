using System;
using System.Collections.Generic;
using DataAsset.Core;
using DataAsset.Core.Struct;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            /// <summary>
            /// Configures and initializes a reorderable list for managing serialized data properties within the ScriptableEditor context.
            /// </summary>
            /// <remarks>
            /// This method creates and sets up the ReorderableList used for managing data within a ScriptableObject.
            /// It defines callbacks for drawing the list header, elements, calculating element heights, and handling element removal.
            /// The reorderable list is configured to allow element dragging and removal, with provisions for displaying a header.
            /// If the serialized data property used to populate the list is null or not found, the method logs an error and aborts setup.
            /// </remarks>
            /// <exception cref="System.NullReferenceException">
            /// Thrown when the serialized property `_allDataProperty` is null or cannot be found.
            /// </exception>
            private void SetupReorderableList()
            {
                  if (_allDataProperty == null)
                  {
                        Debug.LogError("[ScriptableEditor.SetupReorderableList] _allDataProperty is null. Cannot setup list.");

                        return;
                  }

                  // Initialize the reorderable list with the serialized property
                  _reorderableList = new ReorderableList(this.serializedObject,
                              _allDataProperty,
                              draggable: true,
                              displayHeader: true,
                              displayAddButton: false,
                              displayRemoveButton: true)
                  {
                              drawHeaderCallback = DrawListHeader,
                              drawElementCallback = DrawListElement,
                              elementHeightCallback = CalculateElementHeight,
                              onRemoveCallback = OnRemoveElement
                  };
            }

            /// <summary>
            /// Draws the header for the reorderable list in the ScriptableEditor, displaying label and control elements.
            /// </summary>
            /// <param dataName="rect">The rectangle on the GUI where the header will be rendered.</param>
            /// <remarks>
            /// This method renders the "Data Objects" label in the header of the reorderable list along with controls for searching,
            /// sorting, and a button for triggering additional actions.
            /// The method uses spacing and layout constants to ensure proper alignment.
            /// </remarks>
            private void DrawListHeader(Rect rect)
            {
                  // Used to track the horizontal position for drawing elements in the header
                  float currentX = rect.x;

                  // Draw the header label for the list
                  var titleContent = new GUIContent("Data");

                  // Calculate the width of the title label and adjust the current X position accordingly
                  float titleWidth = EditorStyles.label.CalcSize(titleContent).x;
                  EditorGUI.LabelField(new Rect(currentX, rect.y, titleWidth, EditorGUIUtility.singleLineHeight), titleContent);
                  currentX += titleWidth + HeaderSpacing * 2;

                  // The total width of the scan button and its margin
                  const float scanButtonTotalWidth = HeaderScanButtonWidth + HeaderRightMargin;
                  float searchAndSortMaxWidth = rect.width - currentX - scanButtonTotalWidth - HeaderSpacing;

                  // Draw search and sort controls, ensuring they fit within the available width
                  DrawSearchAndSortControls(new Rect(currentX, rect.y, searchAndSortMaxWidth, EditorGUIUtility.singleLineHeight));

                  var scanButtonRect = new Rect(rect.x + rect.width - HeaderScanButtonWidth - HeaderRightMargin,
                              rect.y,
                              HeaderScanButtonWidth,
                              EditorGUIUtility.singleLineHeight);
                  DrawScanButton(scanButtonRect);
            }

            /// <summary>
            /// Draws the search and sort controls within the list header of the ScriptableEditor UI.
            /// </summary>
            /// <param dataName="rect">The rectangular area where the search field and sorting buttons will be drawn.</param>
            /// <remarks>
            /// This method displays a search input field and sorting buttons for the ScriptableEditor ReorderableList.
            /// The search field allows filtering of the displayed items based on the text input.
            /// The sorting buttons provide options for sorting the list by dataName (ascending and descending) or by type.
            /// Each control is positioned dynamically based on the available width within the specified rect.
            /// </remarks>
            private void DrawSearchAndSortControls(Rect rect)
            {
                  // Used to track the horizontal position for drawing elements in the header
                  float currentX = rect.x;

                  // Draw the search field with a maximum width based on the header's width
                  float searchFieldWidth = Mathf.Max(rect.width * HeaderSearchFieldMaxWidthPercentage, HeaderSearchFieldMinWidth);

                  // Ensure the search field does not exceed the available width
                  _searchText = EditorGUI.TextField(new Rect(currentX, rect.y, searchFieldWidth, EditorGUIUtility.singleLineHeight),
                              _searchText,
                              EditorStyles.toolbarSearchField);
                  currentX += searchFieldWidth + HeaderSpacing;

                  // Draw the sorting buttons
                  if (GUI.Button(new Rect(currentX, rect.y, HeaderSortButtonWidth, EditorGUIUtility.singleLineHeight), "Name ▼", EditorStyles.toolbarButton))
                  {
                        ApplySort(SortMode.ByNameAsc);
                  }

                  currentX += HeaderSortButtonWidth;

                  // Draw the sort button for descending dataName order
                  if (GUI.Button(new Rect(currentX, rect.y, HeaderSortButtonWidth, EditorGUIUtility.singleLineHeight), "Name ▲", EditorStyles.toolbarButton))
                  {
                        ApplySort(SortMode.ByNameDesc);
                  }

                  currentX += HeaderSortButtonWidth;

                  // Draw the sort button for sorting by type
                  if (GUI.Button(new Rect(currentX, rect.y, HeaderSortButtonWidth, EditorGUIUtility.singleLineHeight), "Type", EditorStyles.toolbarButton))
                  {
                        ApplySort(SortMode.ByType);
                  }
            }

            /// <summary>
            /// Renders a button in the editor interface that initiates a scan for data dataName usages across scripts in the project.
            /// </summary>
            /// <remarks>
            /// The button's label changes based on the scanning state to indicate whether the scan is currently in progress ("Scanning...")
            /// or ready to be triggered ("Scan Usages").
            /// When the scan is in progress, the button is disabled to prevent additional scans from being triggered simultaneously.
            /// Upon activation, the button calls the `StartScanForDataUsages` method to handle the scanning logic.
            /// </remarks>
            /// <param dataName="rect">The rectangular area within the editor interface where the button is drawn.</param>
            private void DrawScanButton(Rect rect)
            {
                  // Used to display the button label and tooltip
                  GUIContent scanButtonContent = _isScanningUsages
                              ? new GUIContent("Scanning...", "Scanning project for data usages")
                              : new GUIContent("Scan Usages", "Scan scripts for data dataName usages");
                  EditorGUI.BeginDisabledGroup(_isScanningUsages);

                  // Draw the scan button with the appropriate label and tooltip
                  if (GUI.Button(rect, scanButtonContent, EditorStyles.toolbarButton))
                  {
                        StartScanForDataUsages();
                  }

                  EditorGUI.EndDisabledGroup();
            }

            /// <summary>
            /// Renders an element within the reorderable list, including its background, content, and interactive properties.
            /// </summary>
            /// <param dataName="rect">The rectangular area allocated for drawing the element.</param>
            /// <param dataName="index">The index of the element within the serialized property array.</param>
            /// <param dataName="isActive">Indicates whether the element is currently active in the list (e.g., selected).</param>
            /// <param dataName="isFocused">Indicates whether the element currently has focus in the list.</param>
            /// <remarks>
            /// This method handles the rendering out of each data element in the reorderable list.
            /// It accounts for active and focused states and applies conditional styling,
            /// (e.g., dimming for search filtering) and renders custom color bars and nested property fields.
            /// Invalid elements are indicated with a corresponding label.
            /// </remarks>
            private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                  if (!_stylesInitialized)
                  {
                        InitializeEditorStyles();
                  }

                  // Ensure the rect is properly adjusted for the drag handle width
                  SerializedProperty element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);

                  // Check if the element is valid and has a managed reference value
                  if (element.managedReferenceValue is not DataObject dataObject)
                  {
                        EditorGUI.LabelField(rect, "Invalid data element (null reference).");

                        return;
                  }

                  Event currentEvent = Event.current;

                  if (currentEvent.type == EventType.ContextClick && rect.Contains(currentEvent.mousePosition))
                  {
                        GenericMenu menu = new GenericMenu();

                        // Passer l'index comme userData pour que les callbacks sachent quel élément cibler
                        menu.AddItem(new GUIContent("Duplicate Item"), false, HandleDuplicateElementContext, index);
                        menu.AddItem(new GUIContent("Delete Item"), false, HandleDeleteElementContext, index);
                        menu.ShowAsContext();
                        currentEvent.Use(); // Consommer l'événement pour éviter d'autres traitements
                  }

                  // Draw the drag handle for the element
                  bool isDimmed = !string.IsNullOrEmpty(_searchText) &&
                                  !dataObject.dataName.ToLowerInvariant().Contains(_searchText.ToLowerInvariant(), StringComparison.Ordinal);

                  var elementBackgroundRect = new Rect(rect.x + ElementDragHandleWidth, rect.y + 1, rect.width - ElementDragHandleWidth - 1, rect.height - 2);
                  GUI.Box(elementBackgroundRect, GUIContent.none, _blockStyle);

                  var contentRect = new Rect(elementBackgroundRect.x + _blockStyle.padding.left,
                              elementBackgroundRect.y + _blockStyle.padding.top,
                              elementBackgroundRect.width - _blockStyle.padding.horizontal,
                              elementBackgroundRect.height - _blockStyle.padding.vertical);

                  if (isDimmed)
                  {
                        EditorGUI.BeginDisabledGroup(true);
                  }

                  // Draw the color bar on the left side of the element
                  var colorBarRect = new Rect(contentRect.x, contentRect.y, ElementColorBarWidth, contentRect.height);

                  if (_typeColors.TryGetValue(dataObject.GetType(), out Color typeColor))
                  {
                        EditorGUI.DrawRect(colorBarRect, typeColor);
                  }

                  // Draw the content of the element, including header, usage details, and properties
                  var fieldsRect = new Rect(contentRect.x + ElementColorBarWidth + ElementContentPadding,
                              contentRect.y,
                              contentRect.width - ElementColorBarWidth - ElementContentPadding,
                              contentRect.height);

                  // Draw the header line, usage details, dataName error, and properties for the element
                  float currentY = fieldsRect.y;
                  currentY = DrawElementHeaderLine(fieldsRect, fieldsRect.width, currentY, dataObject, element, index);
                  currentY = DrawElementUsageDetails(fieldsRect, fieldsRect.width, currentY, dataObject, index);
                  currentY = DrawElementNameError(fieldsRect, fieldsRect.width, currentY, index);
                  currentY += SmallVerticalSpacing;
                  DrawElementProperties(fieldsRect, fieldsRect.width, currentY, element);

                  if (isDimmed)
                  {
                        EditorGUI.EndDisabledGroup();
                  }
            }

            /// <summary>
            /// Draws the header line for an element in the reorderable list, including dataName, type, and usage information.
            /// </summary>
            /// <param dataName="areaRect">The area rectangle defining the bounds of the current UI element in the list.</param>
            /// <param dataName="availableWidth">The total width available for drawing the element's header line.</param>
            /// <param dataName="startY">The vertical starting position for drawing the header line.</param>
            /// <param dataName="dataObject">The data object associated with the current element to present relevant information.</param>
            /// <param dataName="element">The serialized property representing the list element's data to be rendered.</param>
            /// <param dataName="index">The index of the current element within the reorderable list, used for contextual rendering.</param>
            /// <returns>The vertical position where drawing completed, used to calculate further layout positioning.</returns>
            private float DrawElementHeaderLine(Rect areaRect, float availableWidth, float startY, DataObject dataObject, SerializedProperty element, int index)
            {
                  // Calculate the rectangle for the header line, including padding and spacing
                  var lineRect = new Rect(areaRect.x, startY, availableWidth, EditorGUIUtility.singleLineHeight);

                  // Draw the background for the header line
                  var nameLabelRect = new Rect(lineRect.x, lineRect.y, ElementNameLabelWidth, lineRect.height);
                  EditorGUI.LabelField(nameLabelRect, "Name:");
                  float currentX = nameLabelRect.xMax;

                  // Prepare the label and width for the usage foldout
                  string usageFoldoutLabel = "";
                  float usageFoldoutActualWidth = 0f;
                  List<UsageInfo> usages = null;
                  bool hasUsages = _detailedDataUsages != null && _detailedDataUsages.TryGetValue(dataObject.dataName, out usages) && usages.Count > 0;

                  // If there are usages, prepare the foldout label and calculate its width
                  if (hasUsages)
                  {
                        usageFoldoutLabel = $" ({usages.Count} refs)";

                        usageFoldoutActualWidth = Mathf.Max(EditorStyles.foldout.CalcSize(new GUIContent(usageFoldoutLabel)).x + ElementTypePadding,
                                    ElementUsageFoldoutMinWidth);
                  }

                  // Draw the dataName label
                  var typeNameContent = new GUIContent(dataObject.GetType().Name);

                  // Calculate the width for the type dataName label, including padding
                  float typeNameDisplayWidth = EditorStyles.miniLabel.CalcSize(typeNameContent).x + _elementTypeLabelStyle.padding.horizontal + ElementTypePadding;
                  float nameFieldWidth = availableWidth - nameLabelRect.width - usageFoldoutActualWidth - typeNameDisplayWidth - ElementTypePadding;
                  nameFieldWidth = Mathf.Max(nameFieldWidth, ElementMinNameFieldWidth);

                  // Draw the dataName field, allowing for editing and validation
                  SerializedProperty nameProperty = element.FindPropertyRelative("dataName");

                  // Check if the dataName property exists and is valid
                  if (nameProperty != null)
                  {
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.PropertyField(new Rect(currentX, lineRect.y, nameFieldWidth, lineRect.height), nameProperty, GUIContent.none);

                        if (EditorGUI.EndChangeCheck())
                        {
                              ValidateAllNames();
                        }
                  }
                  else
                  {
                        EditorGUI.LabelField(new Rect(currentX, lineRect.y, nameFieldWidth, lineRect.height), dataObject.dataName);
                  }

                  currentX += nameFieldWidth;

                  // Draw the usage foldout if there are usages
                  if (hasUsages)
                  {
                        _foldoutUsageStates.TryAdd(index, false);

                        _foldoutUsageStates[index] = EditorGUI.Foldout(new Rect(currentX, lineRect.y, usageFoldoutActualWidth, lineRect.height),
                                    _foldoutUsageStates[index],
                                    usageFoldoutLabel,
                                    true,
                                    EditorStyles.foldout);
                        currentX += usageFoldoutActualWidth;
                  }

                  // Draw the type dataName label with padding
                  EditorGUI.LabelField(new Rect(currentX + ElementTypePadding, lineRect.y, typeNameDisplayWidth, lineRect.height),
                              typeNameContent,
                              _elementTypeLabelStyle);

                  return lineRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            }

            /// <summary>
            /// Draws detailed usage information for a specific data object within a specified area.
            /// </summary>
            /// <param dataName="areaRect">The rectangular area in which the usage details will be rendered.</param>
            /// <param dataName="availableWidth">The total width available for rendering content.</param>
            /// <param dataName="startY">The starting Y position within the provided areaRect.</param>
            /// <param dataName="dataObject">The data object for which usage details will be displayed.</param>
            /// <param dataName="index">The index of the current element being processed in the reorderable list.</param>
            /// <returns>The updated Y position after rendering the detailed usage information.</returns>
            private float DrawElementUsageDetails(Rect areaRect, float availableWidth, float startY, DataObject dataObject, int index)
            {
                  float currentY = startY;

                  // Check if the data object is null or has no usages
                  if (!_detailedDataUsages.TryGetValue(dataObject.dataName, out List<UsageInfo> usages) || usages.Count <= 0 ||
                      !_foldoutUsageStates.TryGetValue(index, out bool isExpanded) || !isExpanded)
                  {
                        return currentY;
                  }

                  EditorGUI.indentLevel++;

                  // Draw the header for usage details
                  foreach (UsageInfo usage in usages)
                  {
                        var detailRect = new Rect(areaRect.x, currentY, availableWidth, EditorGUIUtility.singleLineHeight);
                        string goNamePart = string.IsNullOrEmpty(usage.gameObjectName) ? "" : $"on GO '{usage.gameObjectName}' ";
                        string lineInfo = usage.lineNumber > 0 ? $" (L:{usage.lineNumber})" : "";
                        string displayText = $"↳ in '{usage.scriptName}'{lineInfo} {goNamePart}({usage.containerType}: {usage.containerName})";
                        var usageContent = new GUIContent(displayText, $"{usage.scriptPath}\n(Container: {usage.containerPath})");

                        if (GUI.Button(detailRect, usageContent, EditorStyles.label))
                        {
                              var scriptObj = AssetDatabase.LoadAssetAtPath<Object>(usage.scriptPath);

                              if (scriptObj)
                              {
                                    AssetDatabase.OpenAsset(scriptObj, usage.lineNumber > 0 ? usage.lineNumber : -1);
                                    EditorGUIUtility.PingObject(scriptObj);
                              }
                        }

                        currentY += detailRect.height + EditorGUIUtility.standardVerticalSpacing / 2;
                  }

                  EditorGUI.indentLevel--;
                  currentY += EditorGUIUtility.standardVerticalSpacing / 2;

                  return currentY;
            }

            /// <summary>
            /// Draws an error message in the provided area if the element at the given index has a duplicate dataName.
            /// </summary>
            /// <param dataName="areaRect">The rectangle area in which the error message will be displayed.</param>
            /// <param dataName="availableWidth">The width available for drawing the error message.</param>
            /// <param dataName="startY">The starting Y position within the area rectangle for drawing the error message.</param>
            /// <param dataName="index">The index of the element being validated.</param>
            /// <returns>The updated Y position after drawing the error message, including padding.</returns>
            private float DrawElementNameError(Rect areaRect, float availableWidth, float startY, int index)
            {
                  float currentY = startY;

                  // Check if the index is valid and if the dataName is a duplicate
                  if (!_isNameDuplicate.TryGetValue(index, out bool isDuplicate) || !isDuplicate)
                  {
                        return currentY;
                  }

                  // Draw the error message if the dataName is a duplicate
                  var errorRect = new Rect(areaRect.x, currentY, availableWidth, EditorGUIUtility.singleLineHeight);
                  EditorGUI.LabelField(errorRect, "This dataName is already in use.", _errorLabelStyle);
                  currentY += errorRect.height + EditorGUIUtility.standardVerticalSpacing;

                  return currentY;
            }

            /// <summary>
            /// Renders the properties of a serialized element within the specified rectangular layout area.
            /// </summary>
            /// <remarks>
            /// This method iterates through all visible child properties of the given serialized element and draws each one,
            /// skipping the "dataName" property and handling special cases for properties with a ".value" suffix in their paths.
            /// It calculates and adjusts the Y-coordinate incrementally to lay out properties vertically within
            /// the specified area alongside proper spacing between elements.
            /// </remarks>
            /// <param dataName="areaRect">The rectangular area within which the properties are rendered.</param>
            /// <param dataName="availableWidth">The maximum width available for rendering each property's UI.</param>
            /// <param dataName="startY">The initial Y-coordinate from which property rendering begins.</param>
            /// <param dataName="element">The serialized property representing the parent container of the properties to draw.</param>
            private static void DrawElementProperties(Rect areaRect, float availableWidth, float startY, SerializedProperty element)
            {
                  float currentY = startY;
                  SerializedProperty currentProp = element.Copy();
                  bool enterChildren = true;

                  // Iterate through all visible properties of the serialized element
                  while (currentProp.NextVisible(enterChildren))
                  {
                        enterChildren = false;

                        // Check if the current property is the end of the serialized property
                        if (SerializedProperty.EqualContents(currentProp, element.GetEndProperty()))
                        {
                              break;
                        }

                        // Skip the "dataName" property as it is handled separately
                        if (currentProp.name == "dataName")
                        {
                              continue;
                        }

                        // Calculate the rectangle for the current property
                        var propRect = new Rect(areaRect.x, currentY, availableWidth, EditorGUI.GetPropertyHeight(currentProp, true));

                        if (currentProp.propertyPath.EndsWith(".value", StringComparison.OrdinalIgnoreCase))
                        {
                              float originalLabelWidth = EditorGUIUtility.labelWidth;

                              EditorGUIUtility.labelWidth = ElementValueLabelWidth;

                              EditorGUI.PropertyField(propRect, currentProp, true);

                              EditorGUIUtility.labelWidth = originalLabelWidth;
                        }
                        else
                        {
                              EditorGUI.PropertyField(propRect, currentProp, true);
                        }

                        currentY += propRect.height + EditorGUIUtility.standardVerticalSpacing;
                  }
            }

            /// <summary>
            /// Calculates the height required to properly display an element within a reorderable list.
            /// </summary>
            /// <remarks>
            /// This method computes the total height by considering multiple factors, such as element content,
            /// padding, spacing, and error messages associated with the specific data object.
            /// It ensures sufficient vertical space is provided for a clear and consistent display of data.
            /// </remarks>
            /// <param dataName="index">The index of the element in the list for which the height is being calculated.</param>
            /// <returns>The calculated height as a <see cref="float"/> value representing the vertical space required for the element.</returns>
            private float CalculateElementHeight(int index)
            {
                  if (!_stylesInitialized)
                  {
                        InitializeEditorStyles();
                  }

                  // Ensure the index is within the bounds of the serialized property array
                  SerializedProperty element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                  var dataObject = element.managedReferenceValue as DataObject;

                  float height = EditorGUIUtility.singleLineHeight;
                  height += _blockStyle.padding.vertical;
                  height += 4;
                  height += EditorGUIUtility.standardVerticalSpacing * 2;

                  height += CalculateUsageDetailsHeight(index, dataObject?.dataName);
                  height += CalculateNameErrorHeight(index);
                  height += SmallVerticalSpacing;
                  height += CalculatePropertiesHeight(element);

                  return Mathf.Max(height, EditorGUIUtility.singleLineHeight * 2 + _blockStyle.padding.vertical + 4);
            }

            /// <summary>
            /// Calculates the height required to display the usage details for a specific data object in the reorderable list.
            /// </summary>
            /// <param dataName="index">The index of the data element in the reorderable list.</param>
            /// <param dataName="dataObjectName">The dataName of the data object for which usage details are calculated.</param>
            /// <returns>
            /// The calculated height required to display the usage details, including individual lines for data usage and spacing.
            /// Returns 0 if the data object dataName is null, empty, or there are no usage details associated with it.
            /// </returns>
            private float CalculateUsageDetailsHeight(int index, string dataObjectName)
            {
                  float height = 0;

                  if (string.IsNullOrEmpty(dataObjectName))
                  {
                        return 0;
                  }

                  // Check if there are usages for the given data object dataName and if the foldout is expanded
                  if (!_detailedDataUsages.TryGetValue(dataObjectName, out List<UsageInfo> usages) || usages.Count <= 0 ||
                      !_foldoutUsageStates.TryGetValue(index, out bool isExpanded) || !isExpanded)
                  {
                        return height;
                  }

                  height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing / 2) * usages.Count;
                  height += EditorGUIUtility.standardVerticalSpacing / 2;

                  return height;
            }

            /// <summary>
            /// Calculates the height required to display a dataName duplication error message for a specific element in the reorderable list.
            /// </summary>
            /// <param dataName="index">The index of the element in the reorderable list being checked for dataName duplication.</param>
            /// <returns>The height needed to display the error message if a dataName duplication exists for the specified element; otherwise, returns 0.</returns>
            private float CalculateNameErrorHeight(int index)
            {
                  // Check if the index is valid and if the dataName is a duplicate
                  if (_isNameDuplicate.TryGetValue(index, out bool isDuplicate) && isDuplicate)
                  {
                        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                  }

                  return 0;
            }

            /// <summary>
            /// Calculates the total height required to render all visible properties of the given serialized element.
            /// </summary>
            /// <param dataName="element">The serialized property representing the element whose properties' height needs to be calculated.</param>
            /// <returns>The total height in pixels required to display all the visible sub-properties of the provided serialized element.</returns>
            private static float CalculatePropertiesHeight(SerializedProperty element)
            {
                  float height = 0;
                  SerializedProperty currentProp = element.Copy();
                  bool enterChildren = true;

                  // Iterate through all visible properties of the serialized element
                  while (currentProp.NextVisible(enterChildren))
                  {
                        enterChildren = false;

                        // Check if the current property is the end of the serialized property
                        if (SerializedProperty.EqualContents(currentProp, element.GetEndProperty()))
                        {
                              break;
                        }

                        // Skip the "dataName" property as it is handled separately
                        if (currentProp.name == "dataName")
                        {
                              continue;
                        }

                        // Calculate the height for the current property, including its label and spacing
                        height += EditorGUI.GetPropertyHeight(currentProp, true) + EditorGUIUtility.standardVerticalSpacing;
                  }

                  return height;
            }

            /// <summary>
            /// Handles the removal of an element from the reorderable list and updates the serialized property and associated states accordingly.
            /// </summary>
            /// <param dataName="l">The <see cref="ReorderableList"/> instance triggering the removal operation.</param>
            /// <remarks>
            /// This method ensures the proper cleanup of managed reference values and internal usage states when an element is removed.
            /// After updating the serialized array, it validates all names, updates the editor's target asset state, and applies the changes to the serialized object.
            /// </remarks>
            private void OnRemoveElement(ReorderableList l)
            {
                  SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(l.index);

                  // Ensure the element is valid and has a managed reference value before attempting to remove it
                  if (element.managedReferenceValue != null)
                  {
                        element.managedReferenceValue = null;
                  }

                  _foldoutUsageStates.Remove(l.index);

                  l.serializedProperty.DeleteArrayElementAtIndex(l.index);

                  if (l.index >= l.serializedProperty.arraySize && l.serializedProperty.arraySize > 0)
                  {
                        l.index = l.serializedProperty.arraySize - 1;
                  }

                  // Validate all names after removal to ensure no duplicates remain
                  ValidateAllNames();

                  // Mark the target asset as dirty to ensure changes are saved
                  EditorUtility.SetDirty(_targetAsset);
                  this.serializedObject.ApplyModifiedProperties();
                  this.serializedObject.Update();
            }

            private void HandleDuplicateElementContext(object userData)
            {
                  int index = (int)userData;

                  if (index < 0 || index >= _allDataProperty.arraySize)
                  {
                        return;
                  }

                  Undo.RecordObject(_targetAsset, "Duplicate Data Object");

                  SerializedProperty sourceElement = _allDataProperty.GetArrayElementAtIndex(index);

                  if (sourceElement.managedReferenceValue is not DataObject sourceDataObject)
                  {
                        return;
                  }

                  DataObject newInstance;

                  try
                  {
                        newInstance = (DataObject)Activator.CreateInstance(sourceDataObject.GetType());

                        string json = JsonUtility.ToJson(sourceDataObject);
                        JsonUtility.FromJsonOverwrite(json, newInstance);

                        string baseName = string.IsNullOrEmpty(sourceDataObject.dataName) ? "New" + newInstance.GetType().Name : sourceDataObject.dataName;

                        if (!baseName.EndsWith("_Copy", StringComparison.OrdinalIgnoreCase))
                        {
                              baseName += "_Copy";
                        }

                        string potentialName = baseName;
                        int counter = 1;
                        List<string> existingNames = new List<string>();

                        for (int i = 0; i < _allDataProperty.arraySize; ++i)
                        {
                              if (i == index)
                              {
                                    continue;
                              }

                              if (_allDataProperty.GetArrayElementAtIndex(i).managedReferenceValue is DataObject item)
                              {
                                    existingNames.Add(item.dataName);
                              }
                        }

                        existingNames.Add(sourceDataObject.dataName);

                        while (existingNames.Contains(potentialName))
                        {
                              potentialName = $"{baseName}{counter++}";
                        }

                        newInstance.dataName = potentialName;
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"[ScriptableEditor] Error duplicating DataObject: {ex.Message}");

                        return;
                  }

                  _allDataProperty.InsertArrayElementAtIndex(index + 1);
                  SerializedProperty newElementProperty = _allDataProperty.GetArrayElementAtIndex(index + 1);
                  newElementProperty.managedReferenceValue = newInstance;

                  serializedObject.ApplyModifiedProperties();
                  ValidateAllNames();
                  EditorUtility.SetDirty(_targetAsset);
                  Repaint();
            }

            private void HandleDeleteElementContext(object userData)
            {
                  int index = (int)userData;

                  if (index < 0 || index >= _allDataProperty.arraySize)
                  {
                        return;
                  }

                  Undo.RecordObject(_targetAsset, "Delete Data Object");

                  SerializedProperty element = _allDataProperty.GetArrayElementAtIndex(index);

                  if (element.managedReferenceValue != null)
                  {
                        element.managedReferenceValue = null;
                  }

                  _foldoutUsageStates.Remove(index);

                  _allDataProperty.DeleteArrayElementAtIndex(index);

                  serializedObject.ApplyModifiedProperties();
                  ValidateAllNames();
                  EditorUtility.SetDirty(_targetAsset);
                  Repaint();
            }

            private void DrawClearAllButton()
            {
                  if (_allDataProperty == null || _allDataProperty.arraySize == 0)
                  {
                        return;
                  }

                  EditorGUILayout.Space(10);

                  EditorGUILayout.BeginHorizontal();
                  GUILayout.FlexibleSpace();

                  Color originalBgColor = GUI.backgroundColor;
                  GUI.backgroundColor = new Color(1f, 0.6f, 0.6f, 1f);

                  if (GUILayout.Button(new GUIContent(" Clear All Data Objects",
                                              EditorGUIUtility.IconContent("d_TreeEditor.Trash").image,
                                              "Removes ALL data objects from this asset."),
                                  GUILayout.MaxWidth(250)) && EditorUtility.DisplayDialog("Clear All Data Objects",
                                  $"Are you sure you want to remove all {_allDataProperty.arraySize} data objects from this asset? This action can be undone.",
                                  "Clear All",
                                  "Cancel"))
                  {
                        Undo.RecordObject(_targetAsset, "Clear All Data Objects");

                        _allDataProperty.ClearArray();
                        _foldoutUsageStates.Clear();
                        _detailedDataUsages.Clear();
                        ValidateAllNames();

                        EditorUtility.SetDirty(_targetAsset);
                        this.serializedObject.ApplyModifiedProperties();
                        Repaint();
                  }

                  GUI.backgroundColor = originalBgColor;
                  GUILayout.FlexibleSpace();
                  EditorGUILayout.EndHorizontal();
            }
      }
}