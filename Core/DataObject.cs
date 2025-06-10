using System;

namespace DataAsset.Core
{
      [Serializable]
      public abstract class DataObject
      {
            // The name of the data object, used for identification
            public string dataName;

            // Internal event triggered on the data asset when the value of the data object changes
            [field: NonSerialized]
            protected internal event Action<DataObject> OnDataChanged;

#region Constructors

            protected DataObject(string dataName = "Data")
            {
                  this.dataName = dataName;
            }

#endregion

#region Events

            protected virtual void TriggerChange()
            {
                  OnDataChanged?.Invoke(this);
            }

            protected virtual void ClearSubscriptions()
            {
                  OnDataChanged = null;
            }

#endregion
      }
}