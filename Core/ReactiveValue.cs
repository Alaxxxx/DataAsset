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

            /// <summary>
            /// An event invoked whenever the value of the object changes.
            /// </summary>
            /// <remarks>
            /// This event triggers when the property <c>Value</c> is updated with a new value
            /// that is not equal to the current value, as determined by the equality comparer.
            /// Subscribers to this event will be notified with the updated value.
            /// </remarks>
            /// <typeparam name="T">
            /// The type of the value encapsulated in the reactive object.
            /// </typeparam>
            [field: NonSerialized]
            public event Action<T> OnValueChanged;

            protected ReactiveValue()
            {
                  this.value = default;
            }

            protected ReactiveValue(string name, T initialValue = default) : base(name)
            {
                  this.value = initialValue;
            }

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
                        OnValueChanged?.Invoke(this.value);
                        NotifyChange();
                  }
            }

            public override void ClearAllSubscriptions()
            {
                  base.ClearAllSubscriptions();
                  OnValueChanged = null;
            }
      }
}