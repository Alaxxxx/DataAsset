using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base
{
      [Serializable]
      public class ReactiveInt : ReactiveValue<int>
      {
            public ReactiveInt()
            {
            }

            public ReactiveInt(string name, int initialValue) : base(name, initialValue)
            {
            }
      }
}