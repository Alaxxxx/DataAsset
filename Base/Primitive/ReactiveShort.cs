﻿using DataAsset.Core;

namespace DataAsset.Base.Primitive
{
      public class ReactiveShort : ReactiveValue<short>
      {
            public ReactiveShort()
            {
            }

            public ReactiveShort(string dataName, short initialValue) : base(dataName, initialValue)
            {
            }
      }
}