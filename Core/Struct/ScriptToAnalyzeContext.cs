using UnityEditor;

namespace ScriptableAsset.Core.Struct
{
      /// <summary>
      /// Represents a context for analyzing a script's usage within various container types and their associated metadata.
      /// </summary>
      public struct ScriptToAnalyzeContext
      {
            public MonoScript Script;
            public string ReferencingContainerType;
            public string ReferencingContainerName;
            public string ReferencingContainerPath;
            public string SpecificGameObjectName;
      }
}