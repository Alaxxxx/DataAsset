using ScriptableAsset.Core;

namespace ScriptableAsset.Base.Primitive
{
      public class ReactiveShort : ReactiveValue<short>
      {
            public ReactiveShort()
            {
            }

            public ReactiveShort(string name, short initialValue) : base(name, initialValue)
            {
            }
      }
}