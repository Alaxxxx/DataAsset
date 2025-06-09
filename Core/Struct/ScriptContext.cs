using System;
using UnityEditor;

namespace ScriptableAsset.Core.Struct
{
      public struct ScriptContext : IEquatable<ScriptContext>
      {
            public MonoScript Script;
            public string ReferencingContainerType;
            public string ReferencingContainerName;
            public string ReferencingContainerPath;
            public string SpecificGameObjectName;

            public readonly bool Equals(ScriptContext other)
            {
                  return Script == other.Script;
            }
      }
}