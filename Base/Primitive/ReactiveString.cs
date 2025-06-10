using System;
using DataAsset.Core;

namespace DataAsset.Base.Primitive
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