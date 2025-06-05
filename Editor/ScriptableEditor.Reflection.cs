using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScriptableAsset.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private void InitializeDataTypeReflectionAndColors()
            {
                  try
                  {
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

                              foundDataObjectSubclasses.AddRange(typesFromAssembly.Where(static type => type.IsSubclassOf(typeof(DataObject)) && !type.IsAbstract));
                        }

                        _dataTypes = foundDataObjectSubclasses.OrderBy(static t => t.FullName).ToArray();

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

                                                                if (ns.StartsWith("ScriptableAsset.", StringComparison.Ordinal))
                                                                {
                                                                      ns = ns["ScriptableAsset.".Length..];
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
                        Random.State previousRandomState = Random.state;

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
                  }
            }
      }
}