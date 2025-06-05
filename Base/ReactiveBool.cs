using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base
{
      [Serializable]
      public class ReactiveBool : ReactiveValue<bool>
      {
            public ReactiveBool()
            {
            }

            public ReactiveBool(string name, bool initialValue) : base(name, initialValue)
            {
            }
      }
}