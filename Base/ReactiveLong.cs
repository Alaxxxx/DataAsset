using ScriptableAsset.Core;

namespace ScriptableAsset.Base
{
      public class ReactiveLong : ReactiveValue<long>
      {
            public ReactiveLong()
            {
            }

            public ReactiveLong(string name, long initialValue) : base(name, initialValue)
            {
            }
      }
}