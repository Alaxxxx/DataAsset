using System;
using DataAsset.Core;

namespace DataAsset.Base.Collection
{
      [Serializable]
      public class StringList : ReactiveList<string>
      {
            public StringList() : base("New String List")
            {
            }

            public StringList(string dataName) : base(dataName)
            {
            }
      }
}