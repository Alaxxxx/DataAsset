using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Primitive
{
      public class ReactiveLong : ReactiveValue<long>
      {
            public ReactiveLong()
            {
            }

            public ReactiveLong(string dataName, long initialValue) : base(dataName, initialValue)
            {
            }
      }
}