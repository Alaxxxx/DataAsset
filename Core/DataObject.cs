using System;

namespace ScriptableAsset.Core
{
      [Serializable]
      public abstract class DataObject
      {
            public string name;

            /// <summary>
            /// Event that is triggered when any property or value within the <see cref="DataObject"/> instance changes.
            /// </summary>
            /// <remarks>
            /// Subscribing to this event allows listeners to react whenever a change occurs in the data contained
            /// within the concerned <see cref="DataObject"/>. Changes are detected and notification is dispatched
            /// based on the invocation of the <see cref="DataObject.NotifyChange"/> method.
            /// This event is cleared of all subscribers when <see cref="DataObject.ClearAllSubscriptions"/> is called.
            /// </remarks>
            /// <example>
            /// Note: Direct usage examples are not provided. Ensure proper subscription and unsubscription practices
            /// for handling the event to avoid memory leaks or unexpected behavior.
            /// </example>
            [field: NonSerialized]
            public event Action<DataObject> OnAnyValueChanged;

            protected DataObject(string name = "Data")
            {
                  this.name = name;
            }

            protected virtual void NotifyChange()
            {
                  OnAnyValueChanged?.Invoke(this);
            }

            public virtual void ClearAllSubscriptions()
            {
                  OnAnyValueChanged = null;
            }
      }
}