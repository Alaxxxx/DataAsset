using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Collection
{
      [Serializable]
      public class StringList : ReactiveList<string>
      {
            public StringList() : base("New String List")
            {
            }

            public StringList(string name) : base(name)
            {
            }
      }
}