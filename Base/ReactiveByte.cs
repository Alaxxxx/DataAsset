using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base
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