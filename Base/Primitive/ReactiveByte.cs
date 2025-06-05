using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Primitive
{
      [Serializable]
      public class ReactiveByte : ReactiveValue<byte>
      {
            public ReactiveByte()
            {
            }

            public ReactiveByte(string name, byte initialValue) : base(name, initialValue)
            {
            }
      }
}