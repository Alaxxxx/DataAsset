using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptableAsset.Core
{
      [CreateAssetMenu(fileName = "NewScriptableAsset", menuName = "ScriptableObjects/ScriptableAsset", order = 1)]
      public sealed class ScriptableAsset : ScriptableObject
      {
            [SerializeReference] public List<DataObject> assetData = new();

            private Dictionary<string, DataObject> _dataMap;
            private bool _isMapInitialized;

            public event Action<DataObject> OnAnyDataChanged;

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

#if UNITY_EDITOR

            private void OnValidate()
            {
                  _isMapInitialized = false;
            }
#endif

            private void InitializeMapAndSubscribe()
            {
                  if (_isMapInitialized && Application.isPlaying && _dataMap != null && _dataMap.Count == assetData.Count)
                  {
                        return;
                  }

                  _dataMap = new Dictionary<string, DataObject>(assetData.Count);

                  foreach (DataObject data in assetData.Where(static d => d != null))
                  {
                        data.OnAnyValueChanged -= HandleContainedDataChanged;
                  }

                  foreach (DataObject data in assetData.Where(static d => d != null && !string.IsNullOrEmpty(d.name)))
                  {
                        if (!_dataMap.TryAdd(data.name, data))
                        {
                              Debug.LogWarning(
                                          $"[ScriptableAsset: {this.name}] Duplicate data name '{data.name}' found. Only the first instance is accessible via the GetData map.",
                                          this);
                        }

                        data.OnAnyValueChanged += HandleContainedDataChanged;
                  }

                  _isMapInitialized = true;
            }

            private void HandleContainedDataChanged(DataObject changedData)
            {
                  OnAnyDataChanged?.Invoke(changedData);
            }

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

                  return null;
            }

            public DataObject GetData(string dataName)
            {
                  if (!_isMapInitialized)
                  {
                        InitializeMapAndSubscribe();
                  }

                  return _dataMap.GetValueOrDefault(dataName);
            }
      }
}