using System;
using DataAsset.Core;

namespace DataAsset.Base.Primitive
{
      [Serializable]
      public class ReactiveFloat : ReactiveValue<float>
      {
            public ReactiveFloat()
            {
            }

            public ReactiveFloat(string dataName, float initialValue) : base(dataName, initialValue)
            {
            }
      }
}