using System;

namespace DataAsset.Core.Struct
{
      [Serializable]
      public struct UsageInfo : IEquatable<UsageInfo>
      {
            public string scriptName;
            public string scriptPath;
            public string containerType;
            public string containerName;
            public string containerPath;
            public string gameObjectName;
            public int lineNumber;

            public UsageInfo(string scriptName, string scriptPath, string containerType, string containerName, string containerPath, string gameObjectName = null,
                        int lineNumber = 0)
            {
                  this.scriptName = scriptName;
                  this.scriptPath = scriptPath;
                  this.containerType = containerType;
                  this.containerName = containerName;
                  this.containerPath = containerPath;
                  this.gameObjectName = gameObjectName;
                  this.lineNumber = lineNumber;
            }

            public override string ToString()
            {
                  string lineInfo = lineNumber > 0 ? $" L:{lineNumber}" : "";

                  return !string.IsNullOrEmpty(gameObjectName)
                              ? $"Script: {scriptName}{lineInfo} on GO: {gameObjectName} (in {containerType}: {containerName})"
                              : $"Script: {scriptName}{lineInfo} (in {containerType}: {containerName})";
            }

            public bool Equals(UsageInfo other)
            {
                  return scriptName == other.scriptName && scriptPath == other.scriptPath;
            }
      }
}