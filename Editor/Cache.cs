using System;
using System.Collections.Generic;
using System.IO;
using ScriptableAsset.Core.Struct;
using UnityEditor;
using UnityEngine;

namespace ScriptableAsset.Editor
{
      [Serializable]
      public class UsageCacheData
      {
            public List<UsageCacheEntry> entries = new();
      }

      [Serializable]
      public class UsageCacheEntry
      {
            public string dataObjectName;
            public List<UsageInfo> usages = new();
      }

      public sealed partial class ScriptableEditor
      {
            private string GetUsageCacheFilePath()
            {
                  if (!_targetAsset)
                  {
                        return null;
                  }

                  string assetPath = AssetDatabase.GetAssetPath(_targetAsset);

                  if (string.IsNullOrEmpty(assetPath))
                  {
                        Debug.LogWarning($"[ScriptableEditor_Cache] Asset '{_targetAsset.name}' does not have a persistent path.", _targetAsset);

                        return null;
                  }

                  string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

                  if (string.IsNullOrEmpty(assetGuid))
                  {
                        Debug.LogWarning($"[ScriptableEditor_Cache] Could not get GUID for asset '{_targetAsset.name}' at path '{assetPath}'. ", _targetAsset);

                        return null;
                  }

                  string cacheDirectory = Path.Combine("Library", UsageCacheFolderName);

                  if (!Directory.Exists(cacheDirectory))
                  {
                        try
                        {
                              Directory.CreateDirectory(cacheDirectory);
                        }
                        catch (Exception ex)
                        {
                              Debug.LogError($"[ScriptableEditor_Cache] Failed to create cache directory '{cacheDirectory}': {ex.Message}");

                              return null;
                        }
                  }

                  string filePath = Path.Combine(cacheDirectory, $"{assetGuid}.json");

                  return filePath;
            }

            private void SaveUsageCache()
            {
                  string filePath = GetUsageCacheFilePath();

                  if (filePath == null)
                  {
                        Debug.LogWarning($"[ScriptableEditor_Cache] Cannot save cache for '{_targetAsset.name}', file path is invalid.");

                        return;
                  }

                  if (_detailedDataUsages == null)
                  {
                        Debug.LogWarning($"[ScriptableEditor_Cache] _detailedDataUsages is null for '{_targetAsset.name}'. Nothing to save.");

                        return;
                  }

                  var cacheToSave = new UsageCacheData();

                  foreach (KeyValuePair<string, List<UsageInfo>> kvp in _detailedDataUsages)
                  {
                        cacheToSave.entries.Add(new UsageCacheEntry { dataObjectName = kvp.Key, usages = kvp.Value });
                  }

                  try
                  {
                        string json = JsonUtility.ToJson(cacheToSave, true);
                        File.WriteAllText(filePath, json);
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"[ScriptableEditor_Cache] Failed to save usage cache for '{_targetAsset.name}': {ex.Message}");
                  }
            }

            private void LoadUsageCache()
            {
                  _detailedDataUsages.Clear();
                  _foldoutUsageStates.Clear();

                  string filePath = GetUsageCacheFilePath();

                  if (filePath == null)
                  {
                        Debug.LogWarning($"[ScriptableEditor_Cache] Cannot load cache for '{_targetAsset.name}', file path is invalid.");
                        Repaint();

                        return;
                  }

                  if (!File.Exists(filePath))
                  {
                        Repaint();

                        return;
                  }

                  try
                  {
                        string json = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(json))
                        {
                              Debug.LogWarning($"[ScriptableEditor_Cache] Cache file for '{_targetAsset.name}' is empty. Path: {filePath}");
                              Repaint();

                              return;
                        }

                        var loadedCache = JsonUtility.FromJson<UsageCacheData>(json);

                        if (loadedCache is { entries: not null })
                        {
                              foreach (UsageCacheEntry entry in loadedCache.entries)
                              {
                                    _detailedDataUsages[entry.dataObjectName] = entry.usages ?? new List<UsageInfo>();
                              }
                        }
                        else
                        {
                              Debug.LogWarning($"[ScriptableEditor_Cache] Failed to deserialize cache or cache was empty for '{_targetAsset.name}'. Path: {filePath}");
                        }
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"[ScriptableEditor_Cache] Failed to load cache for '{_targetAsset.name}': {ex.Message}. Path: {filePath}");

                        try
                        {
                              File.Delete(filePath);
                        }
                        catch
                        {
                              Debug.LogError($"[ScriptableEditor_Cache] Failed to delete corrupted cache file for '{_targetAsset.name}'. Path: {filePath}");
                        }
                  }

                  Repaint();
            }
      }
}