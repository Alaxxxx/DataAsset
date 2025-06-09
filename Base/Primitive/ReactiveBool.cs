using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Primitive
{
      [Serializable]
      public class ReactiveBool : ReactiveValue<bool>
      {
            public ReactiveBool()
            {
            }

            public ReactiveBool(string dataName, bool initialValue) : base(dataName, initialValue)
            {
            }
      }
}