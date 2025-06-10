using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataAsset.Core
{
      [Serializable]
      public abstract class ReactiveList<TItem> : DataObject, IList<TItem>, IReadOnlyList<TItem>
      {
            [SerializeField] private List<TItem> items = new();

            // Events to notify changes in the list
            [field: NonSerialized] public event Action OnCollectionChanged;
            [field: NonSerialized] public event Action<TItem, int> OnItemAdded;
            [field: NonSerialized] public event Action<TItem, int> OnItemRemoved;
            [field: NonSerialized] public event Action<TItem, TItem, int> OnItemSet;
            [field: NonSerialized] public event Action OnListCleared;

#region Constructors

            protected ReactiveList() : base("New List")
            {
            }

            protected ReactiveList(string dataName) : base(dataName)
            {
                  items = new List<TItem>();
            }

            protected ReactiveList(string dataName, IEnumerable<TItem> initialItems) : base(dataName)
            {
                  items = new List<TItem>(initialItems);
            }

#endregion

#region Methods

            public TItem this[int index]
            {
                  get => items[index];
                  set
                  {
                        if (index < 0 || index >= items.Count)
                        {
                              throw new ArgumentOutOfRangeException(nameof(index));
                        }

                        TItem oldItem = items[index];

                        if (EqualityComparer<TItem>.Default.Equals(oldItem, value))
                        {
                              return;
                        }

                        items[index] = value;
                        OnItemSet?.Invoke(oldItem, value, index);
                        OnCollectionChanged?.Invoke();
                        TriggerChange();
                  }
            }

            public void Add(TItem item)
            {
                  int newIndex = items.Count;
                  items.Add(item);
                  OnItemAdded?.Invoke(item, newIndex);
                  OnCollectionChanged?.Invoke();
                  TriggerChange();
            }

            public void AddRange(IEnumerable<TItem> collection)
            {
                  if (collection == null)
                  {
                        throw new ArgumentNullException(nameof(collection));
                  }

                  List<TItem> itemsToAdd = collection.ToList();

                  if (!itemsToAdd.Any())
                  {
                        return;
                  }

                  items.AddRange(itemsToAdd);

                  OnCollectionChanged?.Invoke();
                  TriggerChange();
            }

            public void Clear()
            {
                  if (items.Count == 0)
                  {
                        return;
                  }

                  items.Clear();
                  OnListCleared?.Invoke();
                  OnCollectionChanged?.Invoke();
                  TriggerChange();
            }


            public void Insert(int index, TItem item)
            {
                  items.Insert(index, item);
                  OnItemAdded?.Invoke(item, index);
                  OnCollectionChanged?.Invoke();
                  TriggerChange();
            }

            public bool Remove(TItem item)
            {
                  int index = IndexOf(item);

                  if (index < 0)
                  {
                        return false;
                  }

                  RemoveAt(index);

                  return true;
            }

            public void RemoveAt(int index)
            {
                  if (index < 0 || index >= items.Count)
                  {
                        throw new ArgumentOutOfRangeException(nameof(index));
                  }

                  TItem removedItem = items[index];
                  items.RemoveAt(index);
                  OnItemRemoved?.Invoke(removedItem, index);
                  OnCollectionChanged?.Invoke();
                  TriggerChange();
            }

#endregion

            public List<TItem> GetRawListCopy() => new(items);

            public void NotifyListChangedExternally()
            {
                  OnCollectionChanged?.Invoke();
                  TriggerChange();
            }

            public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => items.Count;
            public bool IsReadOnly => ((ICollection<TItem>)items).IsReadOnly;

            public bool Contains(TItem item) => items.Contains(item);

            public void CopyTo(TItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

            public int IndexOf(TItem item) => items.IndexOf(item);

            public override string ToString() => $"ReactiveList<{typeof(TItem).Name}> (Count = {Count})";
      }
}