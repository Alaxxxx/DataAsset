using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Primitive
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