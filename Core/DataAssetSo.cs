using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace DataAsset.Core
{
      [CreateAssetMenu(fileName = "NewDataAsset", menuName = "ScriptableObjects/DataAssetSo", order = 1)]
      public sealed class DataAssetSo : ScriptableObject
      {
            // List of DataObject instances that this DataAssetSO holds.
            [SerializeReference] private List<DataObject> dataList = new();

            // Dictionary to map data names to their corresponding DataObject instances.
            private Dictionary<string, DataObject> _dataMap;

            // Flag to check if the data map has been initialized.
            private bool _isMapInitialized;

            // This event is triggered whenever any data object within this DataAssetSO changes.
            public event Action<DataObject> OnAnyDataChanged;

#region Setup

            private void Initialize()
            {
                  if (_isMapInitialized)
                  {
                        return;
                  }

                  _dataMap = new Dictionary<string, DataObject>(dataList.Count);

                  foreach (DataObject data in dataList.Where(static data => data != null && !string.IsNullOrEmpty(data.dataName)))
                  {
                        if (_dataMap.TryAdd(data.dataName, data))
                        {
                              data.OnDataChanged += HandleContainedDataChanged;
                        }
                        else
                        {
                              Debug.LogWarning($"[DataAssetSO: {this.name}] Duplicate data name '{data.dataName}' found.", this);
                        }
                  }

                  _isMapInitialized = true;
            }

#endregion

#region Dispose

            private void OnDisable()
            {
                  if (dataList != null)
                  {
                        foreach (DataObject dataObj in dataList.Where(static dataObj => dataObj != null))
                        {
                              dataObj.OnDataChanged -= HandleContainedDataChanged;
                        }
                  }

                  OnAnyDataChanged = null;
            }

#endregion

#region Getters

            public T GetData<T>(string dataName) where T : DataObject
            {
                  if (!_isMapInitialized)
                  {
                        Initialize();
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
                        Initialize();
                  }

                  return _dataMap.GetValueOrDefault(dataName);
            }

#endregion

#region Setters

            public void AddData(DataObject data)
            {
                  if (data == null || string.IsNullOrEmpty(data.dataName))
                  {
                        Debug.LogWarning($"[DataAssetSO: {this.name}] Attempted to add null or unnamed data object.", this);

                        return;
                  }

                  if (_dataMap.ContainsKey(data.dataName))
                  {
                        Debug.LogWarning($"[DataAssetSO: {this.name}] Data with name '{data.dataName}' already exists. Skipping addition.", this);

                        return;
                  }

                  dataList.Add(data);
                  _dataMap[data.dataName] = data;
                  _isMapInitialized = false;
                  data.OnDataChanged += HandleContainedDataChanged;
            }

#endregion

#region Events

            private void HandleContainedDataChanged(DataObject changedData)
            {
                  OnAnyDataChanged?.Invoke(changedData);
            }

#endregion

#if UNITY_EDITOR

            private void OnValidate()
            {
                  _isMapInitialized = false;
            }

            [UsedImplicitly]
            private void OnOnAnyDataChanged(DataObject obj)
            {
                  OnAnyDataChanged?.Invoke(obj);
            }
#endif
      }
}