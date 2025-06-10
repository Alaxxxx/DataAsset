using System;
using DataAsset.Core;

namespace DataAsset.Base.Primitive
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