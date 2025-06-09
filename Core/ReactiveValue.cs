using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableAsset.Core
{
      [Serializable]
      public abstract class ReactiveValue<T> : DataObject
      {
            [SerializeField]
            private T value;

            [field: NonSerialized]
            public event Action<T> OnValueChanged;

#region Constructors

            protected ReactiveValue()
            {
                  this.value = default;
            }

            protected ReactiveValue(string dataName, T initialValue = default) : base(dataName)
            {
                  this.value = initialValue;
            }

#endregion

            public T Value
            {
                  get => value;
                  set
                  {
                        if (EqualityComparer<T>.Default.Equals(this.value, value))
                        {
                              return;
                        }

                        this.value = value;
                        OnValueChanged?.Invoke(value);
                        TriggerChange();
                  }
            }
      }
}