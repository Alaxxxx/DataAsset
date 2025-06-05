using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableAsset.Core;
using UnityEngine;

namespace ScriptableAsset
{
      [CreateAssetMenu(fileName = "ScriptableAsset", menuName = "ScriptableObjects/ScriptableAsset", order = 1)]
      public sealed class ScriptableAsset : ScriptableObject
      {
            [SerializeReference] public List<DataObject> assetData = new();

            private Dictionary<string, DataObject> _dataMap;
            private bool _isMapInitialized;

            // Event to notify when any contained DataObject changes.
            public event Action<DataObject> OnAnyDataChanged;

            private void OnEnable()
            {
                  // Build the map and subscribe to changes when the asset is enabled
                  InitializeMapAndSubscribe();
            }

            private void OnDisable()
            {
                  if (assetData == null)
                  {
                        return;
                  }

                  foreach (DataObject dataObj in assetData.Where(static dataObj => dataObj != null))
                  {
                        dataObj.OnAnyValueChanged -= HandleContainedDataChanged;
                  }

                  OnAnyDataChanged = null;
            }


            private void InitializeMapAndSubscribe()
            {
                  if (_isMapInitialized && Application.isPlaying && _dataMap != null && _dataMap.Count == assetData.Count)
                  {
                        return;
                  }

                  _dataMap = new Dictionary<string, DataObject>(assetData.Count);

                  foreach (DataObject data in assetData.Where(static data => data != null && !string.IsNullOrEmpty(data.name)))
                  {
                        if (!_dataMap.TryAdd(data.name, data))
                        {
                              Debug.LogWarning(
                                          $"[ScriptableAsset: {this.name}] Duplicate data name '{data.name}' found. Only the first instance is accessible via the GetData map.",
                                          this);
                        }

                        data.OnAnyValueChanged -= HandleContainedDataChanged;
                        data.OnAnyValueChanged += HandleContainedDataChanged;
                  }

                  _isMapInitialized = true;
            }

            private void HandleContainedDataChanged(DataObject changedData)
            {
                  OnAnyDataChanged?.Invoke(changedData);
            }

            /// <summary>
            /// Retrieves the data object of a specified type by its name from the asset's internal data map.
            /// </summary>
            /// <typeparam name="T">The type of DataObject to retrieve.</typeparam>
            /// <param name="dataName">The name of the data object to retrieve.</param>
            /// <returns>
            /// Returns the data object of type <typeparamref name="T"/> if found, otherwise returns null.
            /// Logs a warning if the data object is not found or if its type does not match <typeparamref name="T"/>.
            /// </returns>
            public T GetData<T>(string dataName) where T : DataObject
            {
                  if (!_isMapInitialized)
                  {
                        InitializeMapAndSubscribe();
                  }

                  if (_dataMap.TryGetValue(dataName, out DataObject data) && data is T typedData)
                  {
                        return typedData;
                  }

                  Debug.LogWarning($"[ScriptableAsset: {this.name}] Data '{dataName}' of type {typeof(T).Name} not found.", this);

                  return null;
            }

            /// <summary>
            /// Retrieves the data object by its name from the asset's internal data map.
            /// </summary>
            /// <param name="dataName">The name of the data object to retrieve.</param>
            /// <returns>
            /// Returns the data object if found, otherwise returns null.
            /// Logs a warning if the data object is not found.
            /// </returns>
            public DataObject GetData(string dataName)
            {
                  if (!_isMapInitialized)
                  {
                        InitializeMapAndSubscribe();
                  }

                  if (_dataMap.TryGetValue(dataName, out DataObject data))
                  {
                        return data;
                  }

                  Debug.LogWarning($"[ScriptableAsset: {this.name}] Data '{dataName}' not found.", this);

                  return null;
            }
      }
}