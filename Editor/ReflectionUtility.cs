using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataAsset.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            /// <summary>
            /// Initializes the reflection process to gather all data types that inherit from the `DataObject` class
            /// and organizes them for use in the editor.
            /// This includes ordering the types, extracting display names, and setting up type-color associations for visual representation.
            /// </summary>
            /// <remarks>
            /// This method scans all loaded assemblies to find non-abstract subclasses of `DataObject`,
            /// orders them by their full type dataName, and processes them to prepare for usage in the ScriptableEditor UI.
            /// If an error occurs during the reflection process, the data type arrays and collections
            /// are reset to empty states, and an error message is logged.
            /// </remarks>
            /// <exception cref="System.Exception">
            /// Captures any exception that might occur during the process
            /// of assemblies or type scanning and logs the error to the console.
            /// </exception>
            private void InitializeDataTypeReflectionAndColors()
            {
                  try
                  {
                        // Get all assemblies currently loaded in the application domain
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        var foundDataObjectSubclasses = new List<Type>();

                        foreach (Assembly assembly in assemblies)
                        {
                              Type[] typesFromAssembly;

                              try
                              {
                                    typesFromAssembly = assembly.GetTypes();
                              }
                              catch
                              {
                                    continue;
                              }

                              // Filter types to find those that are subclasses of DataObject and not abstract
                              foundDataObjectSubclasses.AddRange(typesFromAssembly.Where(static type => type.IsSubclassOf(typeof(DataObject)) && !type.IsAbstract));
                        }

                        // Sort the found types by their full dataName
                        _dataTypes = foundDataObjectSubclasses.OrderBy(static t => t.FullName).ToArray();

                        // Create a display dataName for each type, removing the "Reactive" prefix and formatting namespaces
                        _dataTypeDisplayNames = _dataTypes.Select(static t =>
                                                          {
                                                                string originalName = t.Name;
                                                                string displayName = originalName;
                                                                const string prefixToRemove = "Reactive";

                                                                if (originalName.StartsWith(prefixToRemove, StringComparison.Ordinal) &&
                                                                    originalName.Length > prefixToRemove.Length)
                                                                {
                                                                      displayName = originalName[prefixToRemove.Length..];
                                                                }

                                                                string ns = t.Namespace ?? "";

                                                                if (ns.StartsWith("DataAsset.", StringComparison.Ordinal))
                                                                {
                                                                      ns = ns["DataAsset.".Length..];
                                                                }

                                                                if (ns == "Base")
                                                                {
                                                                      return $"Basic Types/{displayName}";
                                                                }

                                                                return string.IsNullOrEmpty(ns)
                                                                            ? $"Other Types/{displayName}"
                                                                            : $"{ns.Replace(".", "/", StringComparison.Ordinal)}/{displayName}";
                                                          })
                                                          .ToArray();

                        _typeColors.Clear();

                        // If no data types were found, exit early
                        if (_dataTypes.Length <= 0)
                        {
                              return;
                        }

                        // Initialize random state to ensure consistent color generation
                        Random.State previousRandomState = Random.state;

                        // Generate a unique color for each data type based on its dataName
                        foreach (Type type in _dataTypes)
                        {
                              Random.InitState(type.FullName?.GetHashCode() ?? type.Name.GetHashCode());
                              _typeColors[type] = Color.HSVToRGB(Random.value, 0.65f, 0.90f);
                        }

                        Random.state = previousRandomState;
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"[ScriptableEditor] Error during reflection initialization: {ex}");

                        _dataTypes = Array.Empty<Type>();
                        _dataTypeDisplayNames = Array.Empty<string>();
                  }
            }
      }
}