using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ScriptableAsset.Core;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      public sealed partial class ScriptableEditor
      {
            private void StartScanForDataUsages()
            {
                  if (_isScanningUsages)
                  {
                        Debug.LogWarning("[ScriptableEditor] Usage scan is already in progress.");

                        return;
                  }

                  if (_scanCoroutine != null)
                  {
                        EditorCoroutineUtility.StopCoroutine(_scanCoroutine);
                  }

                  _scanCoroutine = EditorCoroutineUtility.StartCoroutine(ScanForDataUsagesCoroutine(), this);
            }

            private IEnumerator ScanForDataUsagesCoroutine()
            {
                  if (!_targetAsset || _allDataProperty == null)
                  {
                        Debug.LogError("[ScriptableEditor] Target asset or data property is null. Scan aborted.");
                        _isScanningUsages = false;
                        Repaint();

                        yield break;
                  }

                  _isScanningUsages = true;
                  _detailedDataUsages.Clear();
                  Repaint();

                  var currentDataObjects = new List<DataObject>();

                  for (int i = 0; i < _allDataProperty.arraySize; i++)
                  {
                        if (_allDataProperty.GetArrayElementAtIndex(i).managedReferenceValue is DataObject dataObj && !string.IsNullOrEmpty(dataObj.name))
                        {
                              _detailedDataUsages[dataObj.name] = new List<UsageInfo>();
                              currentDataObjects.Add(dataObj);
                        }
                  }

                  if (!currentDataObjects.Any())
                  {
                        Debug.Log("[ScriptableEditor] No data objects in the current asset to scan for.");
                        _isScanningUsages = false;
                        Repaint();

                        yield break;
                  }

                  var scriptsToAnalyzeWithContext = new List<ScriptToAnalyzeContext>();
                  string targetAssetPath = AssetDatabase.GetAssetPath(_targetAsset);

                  string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

                  for (int i = 0; i < prefabGuids.Length; i++)
                  {
                        string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);

                        EditorUtility.DisplayProgressBar("Scanning Referencing Assets",
                                    $"Checking Prefab: {Path.GetFileName(prefabPath)} ({i + 1}/{prefabGuids.Length})",
                                    (float)i / prefabGuids.Length);
                        var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                        if (prefabRoot)
                        {
                              foreach (MonoBehaviour component in prefabRoot.GetComponentsInChildren<MonoBehaviour>(true))
                              {
                                    if (!component)
                                    {
                                          continue;
                                    }

                                    var soComponent = new SerializedObject(component);
                                    SerializedProperty propIterator = soComponent.GetIterator();

                                    while (propIterator.NextVisible(true))
                                    {
                                          if (propIterator.propertyType != SerializedPropertyType.ObjectReference || propIterator.objectReferenceValue != _targetAsset)
                                          {
                                                continue;
                                          }

                                          MonoScript ms = MonoScript.FromMonoBehaviour(component);

                                          if (ms)
                                          {
                                                scriptsToAnalyzeWithContext.Add(new ScriptToAnalyzeContext
                                                {
                                                            Script = ms,
                                                            ReferencingContainerType = "Prefab",
                                                            ReferencingContainerName = prefabRoot.name,
                                                            ReferencingContainerPath = prefabPath,
                                                            SpecificGameObjectName = component.gameObject.name
                                                });
                                          }

                                          break;
                                    }

                                    soComponent.Dispose();
                              }
                        }

                        yield return null;
                  }

                  string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");

                  for (int i = 0; i < soGuids.Length; i++)
                  {
                        string soPath = AssetDatabase.GUIDToAssetPath(soGuids[i]);

                        if (soPath == targetAssetPath)
                        {
                              continue;
                        }

                        EditorUtility.DisplayProgressBar("Scanning Referencing Assets",
                                    $"Checking SO: {Path.GetFileName(soPath)} ({i + 1}/{soGuids.Length})",
                                    (float)i / soGuids.Length);
                        var soInstance = AssetDatabase.LoadAssetAtPath<ScriptableObject>(soPath);

                        if (soInstance)
                        {
                              var soAsset = new SerializedObject(soInstance);
                              SerializedProperty propIterator = soAsset.GetIterator();

                              while (propIterator.NextVisible(true))
                              {
                                    if (propIterator.propertyType == SerializedPropertyType.ObjectReference && propIterator.objectReferenceValue == _targetAsset)
                                    {
                                          MonoScript ms = MonoScript.FromScriptableObject(soInstance);

                                          if (ms)
                                          {
                                                scriptsToAnalyzeWithContext.Add(new ScriptToAnalyzeContext
                                                {
                                                            Script = ms,
                                                            ReferencingContainerType = "ScriptableObject",
                                                            ReferencingContainerName = soInstance.name,
                                                            ReferencingContainerPath = soPath,
                                                            SpecificGameObjectName = null
                                                });
                                          }

                                          break;
                                    }
                              }

                              soAsset.Dispose();
                        }

                        yield return null;
                  }

                  EditorUtility.DisplayProgressBar("Scanning Referencing Assets", "Checking open scene(s)...", 0.9f);

                  for (int i = 0; i < SceneManager.sceneCount; i++)
                  {
                        Scene scene = SceneManager.GetSceneAt(i);

                        if (scene.IsValid() && scene.isLoaded)
                        {
                              foreach (GameObject rootGo in scene.GetRootGameObjects())
                              {
                                    foreach (MonoBehaviour component in rootGo.GetComponentsInChildren<MonoBehaviour>(true))
                                    {
                                          if (!component)
                                          {
                                                continue;
                                          }

                                          var soComponent = new SerializedObject(component);
                                          SerializedProperty propIterator = soComponent.GetIterator();

                                          while (propIterator.NextVisible(true))
                                          {
                                                if (propIterator.propertyType != SerializedPropertyType.ObjectReference ||
                                                    propIterator.objectReferenceValue != _targetAsset)
                                                {
                                                      continue;
                                                }

                                                MonoScript ms = MonoScript.FromMonoBehaviour(component);

                                                if (ms)
                                                {
                                                      scriptsToAnalyzeWithContext.Add(new ScriptToAnalyzeContext
                                                      {
                                                                  Script = ms,
                                                                  ReferencingContainerType = "Scene",
                                                                  ReferencingContainerName = scene.name,
                                                                  ReferencingContainerPath = scene.path,
                                                                  SpecificGameObjectName = component.gameObject.name
                                                      });
                                                }

                                                break;
                                          }

                                          soComponent.Dispose();
                                    }
                              }
                        }

                        yield return null;
                  }

                  EditorUtility.ClearProgressBar();

                  if (!scriptsToAnalyzeWithContext.Any())
                  {
                        Debug.Log($"[ScriptableEditor] No scripts found referencing the asset '{_targetAsset.name}'.");
                        _isScanningUsages = false;
                        Repaint();

                        yield break;
                  }

                  for (int k = 0; k < scriptsToAnalyzeWithContext.Count; k++)
                  {
                        ScriptToAnalyzeContext context = scriptsToAnalyzeWithContext[k];
                        string scriptPath = AssetDatabase.GetAssetPath(context.Script);
                        string scriptName = Path.GetFileName(scriptPath);

                        EditorUtility.DisplayProgressBar("Analyzing Scripts",
                                    $"Processing: {scriptName} ({k + 1}/{scriptsToAnalyzeWithContext.Count})",
                                    (float)k / scriptsToAnalyzeWithContext.Count);

                        if (string.IsNullOrEmpty(scriptPath) || !scriptPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                        {
                              continue;
                        }

                        try
                        {
                              string scriptContent = File.ReadAllText(scriptPath);

                              foreach (DataObject dataItem in currentDataObjects)
                              {
                                    if (string.IsNullOrEmpty(dataItem.name))
                                    {
                                          continue;
                                    }

                                    string escapedName = Regex.Escape(dataItem.name);

                                    var usagePrototype = new UsageInfo(scriptName,
                                                scriptPath,
                                                context.ReferencingContainerType,
                                                context.ReferencingContainerName,
                                                context.ReferencingContainerPath,
                                                context.SpecificGameObjectName);

                                    string patternNonGeneric = $@"\.GetData\s*\(\s*\""{escapedName}\""\s*\)";
                                    MatchCollection matchesNonGeneric = Regex.Matches(scriptContent, patternNonGeneric);

                                    foreach (Match unused in matchesNonGeneric)
                                    {
                                          _detailedDataUsages[dataItem.name].Add(usagePrototype);
                                    }

                                    string patternGeneric = $@"\.GetData\s*<[^>]+>\s*\(\s*\""{escapedName}\""\s*\)";
                                    MatchCollection matchesGeneric = Regex.Matches(scriptContent, patternGeneric);

                                    foreach (Match unused in matchesGeneric)
                                    {
                                          _detailedDataUsages[dataItem.name].Add(usagePrototype);
                                    }
                              }
                        }
                        catch (Exception ex)
                        {
                              Debug.LogWarning($"[ScriptableEditor] Could not read or parse script {scriptPath}: {ex.Message}");
                        }

                        yield return null;
                  }

                  EditorUtility.ClearProgressBar();
                  _isScanningUsages = false;
                  long totalUsagesFound = _detailedDataUsages.Sum(static list => list.Value.Count);

                  Debug.Log(
                              $"[ScriptableEditor] Script scan complete. Found {totalUsagesFound} potential data usages across {scriptsToAnalyzeWithContext.Count} script contexts.");
                  Repaint();
            }
      }


}