using System;
using DataAsset.Core;

namespace DataAsset.Base.Primitive
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