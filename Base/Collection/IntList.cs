using System;
using DataAsset.Core;

namespace DataAsset.Base.Collection
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