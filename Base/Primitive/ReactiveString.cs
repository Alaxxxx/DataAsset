using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Primitive
{
      [Serializable]
      public class ReactiveString : ReactiveValue<string>
      {
            public ReactiveString()
            {
            }

            public ReactiveString(string name, string initialValue) : base(name, initialValue)
            {
            }
      }
}