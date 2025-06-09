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

            public ReactiveString(string dataName, string initialValue) : base(dataName, initialValue)
            {
            }
      }
}