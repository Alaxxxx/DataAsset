using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptableAsset.Core
{
      /// <summary>
      /// Represents a reactive list that notifies subscribers about changes in its content such as additions, removals, and updates.
      /// </summary>
      /// <typeparam name="TItem">The type of the elements in the list.</typeparam>
      [Serializable]
      public abstract class ReactiveList<TItem> : DataObject, IList<TItem>, IReadOnlyList<TItem>
      {
            [SerializeField]
            private List<TItem> items = new();

            [field: NonSerialized] public event Action OnCollectionChanged;
            [field: NonSerialized] public event Action<TItem, int> OnItemAdded;
            [field: NonSerialized] public event Action<TItem, int> OnItemRemoved;
            [field: NonSerialized] public event Action<TItem, TItem, int> OnItemSet;
            [field: NonSerialized] public event Action OnListCleared;

            protected ReactiveList() : base("New List")
            {
            }

            protected ReactiveList(string name) : base(name)
            {
                  items = new List<TItem>();
            }

            protected ReactiveList(string name, IEnumerable<TItem> initialItems) : base(name)
            {
                  items = new List<TItem>(initialItems);
            }

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
                        NotifyChange();
                  }
            }

            public int Count => items.Count;
            public bool IsReadOnly => ((ICollection<TItem>)items).IsReadOnly;

            public void Add(TItem item)
            {
                  int newIndex = items.Count;
                  items.Add(item);
                  OnItemAdded?.Invoke(item, newIndex);
                  OnCollectionChanged?.Invoke();
                  NotifyChange();
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
                  NotifyChange();
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
                  NotifyChange();
            }

            public bool Contains(TItem item) => items.Contains(item);

            public void CopyTo(TItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

            public int IndexOf(TItem item) => items.IndexOf(item);

            public void Insert(int index, TItem item)
            {
                  items.Insert(index, item);
                  OnItemAdded?.Invoke(item, index);
                  OnCollectionChanged?.Invoke();
                  NotifyChange();
            }

            public bool Remove(TItem item)
            {
                  int index = IndexOf(item);

                  if (index < 0)
                  {
                        return false;
                  }

                  bool removed = items.Remove(item);

                  if (removed)
                  {
                        OnItemRemoved?.Invoke(item, index);
                        OnCollectionChanged?.Invoke();
                        NotifyChange();
                  }

                  return removed;
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
                  NotifyChange();
            }

            public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override void ClearAllSubscriptions()
            {
                  base.ClearAllSubscriptions();
                  OnCollectionChanged = null;
                  OnItemAdded = null;
                  OnItemRemoved = null;
                  OnItemSet = null;
                  OnListCleared = null;
            }

            public List<TItem> GetRawListCopy() => new(items);

            public void NotifyListChangedExternally()
            {
                  OnCollectionChanged?.Invoke();
                  NotifyChange();
            }

            public override string ToString() => $"ReactiveList<{typeof(TItem).Name}> (Count = {Count})";
      }
}