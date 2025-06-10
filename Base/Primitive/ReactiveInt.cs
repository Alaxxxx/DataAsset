using System;
using DataAsset.Core;

namespace DataAsset.Base.Primitive
{
      [Serializable]
      public class ReactiveInt : ReactiveValue<int>
      {
            public ReactiveInt()
            {
            }

            public ReactiveInt(string dataName, int initialValue) : base(dataName, initialValue)
            {
            }
      }
}