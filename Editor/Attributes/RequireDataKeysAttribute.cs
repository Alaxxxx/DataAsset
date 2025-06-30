using System;
using UnityEngine;

namespace DataAsset.Editor.Attributes
{
      [AttributeUsage(AttributeTargets.Field)]
      public sealed class RequireDataKeysAttribute : PropertyAttribute
      {
            public readonly string[] RequiredKeys;

            public RequireDataKeysAttribute(params string[] requiredKeys)
            {
                  RequiredKeys = requiredKeys;
            }
      }
}