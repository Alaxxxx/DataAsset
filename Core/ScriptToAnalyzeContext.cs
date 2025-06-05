using UnityEditor;

namespace ScriptableAsset.Core
{
      public struct ScriptToAnalyzeContext
      {
            public MonoScript Script;
            public string ReferencingContainerType;
            public string ReferencingContainerName;
            public string ReferencingContainerPath;
            public string SpecificGameObjectName;
      }
}