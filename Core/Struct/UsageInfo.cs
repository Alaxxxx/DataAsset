namespace ScriptableAsset.Core.Struct
{
      /// <summary>
      /// Represents detailed information about a script's usage within a specific context.
      /// </summary>
      /// <remarks>
      /// This struct provides metadata about the location, container, and context in which a script is used.
      /// It is primarily used to track references and identify usage patterns within Unity-based projects.
      /// </remarks>
      public readonly struct UsageInfo
      {
            public readonly string ScriptName;
            public readonly string ScriptPath;

            public readonly string ContainerType;
            public readonly string ContainerName;
            public readonly string ContainerPath;
            public readonly string GameObjectName;

            public UsageInfo(string scriptName, string scriptPath, string containerType, string containerName, string containerPath, string gameObjectName = null)
            {
                  ScriptName = scriptName;
                  ScriptPath = scriptPath;
                  ContainerType = containerType;
                  ContainerName = containerName;
                  ContainerPath = containerPath;
                  GameObjectName = gameObjectName;
            }

            public override string ToString()
            {
                  return !string.IsNullOrEmpty(GameObjectName)
                              ? $"Script: {ScriptName} on GameObject: {GameObjectName} (in {ContainerType}: {ContainerName})"
                              : $"Script: {ScriptName} (in {ContainerType}: {ContainerName})";
            }
      }
}