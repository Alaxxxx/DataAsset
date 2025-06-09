using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Collection
{
      [Serializable]
      public class IntList : ReactiveList<int>
      {
            public IntList() : base("New Int List")
            {
            }

            public IntList(string dataName) : base(dataName)
            {
            }
      }
}