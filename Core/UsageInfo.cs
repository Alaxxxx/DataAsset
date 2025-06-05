namespace ScriptableAsset.Core
{
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