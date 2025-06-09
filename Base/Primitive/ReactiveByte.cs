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

            public ReactiveByte(string dataName, byte initialValue) : base(dataName, initialValue)
            {
            }
      }
}