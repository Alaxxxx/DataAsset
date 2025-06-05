using System;
using ScriptableAsset.Core;

namespace ScriptableAsset.Base
{
      [Serializable]
      public class ReactiveFloat : ReactiveValue<float>
      {
            public ReactiveFloat()
            {
            }

            public ReactiveFloat(string name, float initialValue) : base(name, initialValue)
            {
            }
      }
}