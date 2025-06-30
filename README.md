# Unity DataAsset

This project provides a framework for implementing a data-oriented design in Unity. It uses `ScriptableObjects` as a robust alternative to the Singleton pattern for managing shared state and data, ensuring a clear separation between data and logic.

## Core Concepts

-   **`DataAssetSo`**: A `ScriptableObject` that acts as a container for a list of data objects.
-   **`DataObject`**: The abstract base class for all data entries. The system uses `[SerializeReference]` to store different data types in one collection.
-   **`ReactiveValue<T>`**: A `DataObject` for a single value (e.g., `int`, `bool`) that invokes an `OnValueChanged` event when its data changes.
-   **`ReactiveList<T>`**: A `DataObject` for a list of items, providing events for collection changes (`OnItemAdded`, `OnItemRemoved`, etc.).

## Key Features

✨ **Powerful & Extensible Editor**
-   A complete custom editor is provided to facilitate the creation and management of data assets.
-   The system automatically detects and integrates any new data class. Simply inherit from `ReactiveValue<T>` or `ReactiveList<T>`, and it will immediately appear as an option in the editor.
-   A set of basic reactive types (`Int`, `String`, `Bool`, `Float`, etc.) and collections (`IntList`, `StringList`) are already included to get you started.

🔧 **Robust Tooling & Validation**
-   **Usage Scanning**: Find every reference to a data key across your entire project, including scripts, prefabs, and scenes.
-   **Dependency Validation**: Use the `[RequireDataKeys]` attribute on a `DataAssetSo` field. The inspector will warn you at edit-time if the referenced asset is missing any of the specified keys.
-   **Automatic Key Generation**: To avoid error-prone "magic strings", the editor includes a tool to generate a static C# class containing all your data keys as `const string`. This ensures type-safe access in your code.

## How to Use

### 1. Create a DataAsset

Create an asset via the **Assets > Create > ScriptableObjects > DataAssetSo** menu.

![Screenshot_1](https://github.com/user-attachments/assets/3a765e09-0b6c-42c0-a1cf-f405c46a51e2)

### 2. Add Data Objects

Select the `DataAssetSo` file to open the custom inspector. Use the "Add New Data" section to add `DataObject` entries.

![Screenshot_2](https://github.com/user-attachments/assets/f9b244ff-3a9e-4e3f-bbfa-e7aeb63cd398)

Already included reactive types are located in "Base" within the dropdown wich contains a "Primitive" and a "Collection" subfolder.
Each custom reactive type will have its own dropdown category depending on the namespace. So for example, this custom type :

```csharp
namespace Custom
{
      [Serializable]
      public class ReactiveVector3 : ReactiveValue<Vector3>
      {
            public ReactiveVector3()
            {
            }

            public ReactiveVector3(string dataName, Vector3 initialValue) : base(dataName, initialValue)
            {
            }
      }
}
```

Will be located in a "Custom" subfolder.

![Screenshot_3](https://github.com/user-attachments/assets/e7824fc7-6790-4b38-8c7c-ec474cb493ac)

Each DataObject within a DataAssetSo is accessible by its name, as it is bound to a dictionary for fast lookups when the asset is enabled.

![Screenshot_4](https://github.com/user-attachments/assets/2dc0a0fc-b48a-46be-a161-788e2272e8d5)

### 3. Access Data in a Script

Reference the `DataAssetSo` in a component and use `GetData<T>()` to access data.

```csharp
using DataAsset.Base.Primitive;
using UnityEngine;

public sealed class PlayerManager : MonoBehaviour
{
      public DataAssetSo playerData; // Reference to the DataAsset containing player data
      private ReactiveFloat playerHealth; // ReactiveFloat to track player health

      private void OnEnable()
      {
            // Grab the ReactiveFloat from the DataAsset
            playerHealth = playerData.GetData<ReactiveFloat>("health");

            // As the value of reactive data is wrapped in a class,
            // we need to access the Value property to get the actual float value
            if (Mathf.Abs(playerHealth.Value) < 0.0001f)
            {
                  playerHealth.Value = 100f;
            }

            // Subscribe to the OnValueChanged event of player health
            playerHealth.OnValueChanged += HandlePlayerHealthChanged;

            // This event is triggered whenever any data object within the DataAsset changes
            playerData.OnAnyDataChanged += HandlePlayerChanged;
      }

      private void OnDisable()
      {
            playerHealth.OnValueChanged -= HandlePlayerHealthChanged;
      }

      private void HandlePlayerHealthChanged(float value)
      {
            Debug.Log($"Player health changed to: {value}");
      }

      private void HandlePlayerChanged(DataObject data)
      {
            Debug.Log($"Data changed: {data.dataName}");
      }
}
```

## Extending the System

To create a custom data type, simply create a new class that inherits from `ReactiveValue<T>` or `ReactiveList<T>`. The system will automatically detect and integrate it.

```csharp
using System;
using DataAsset.Core;
using UnityEngine;

namespace Custom
{
      [Serializable]
      public class ReactiveVector3 : ReactiveValue<Vector3>
      {
            // This constructor is required for Unity's serialization system.
            public ReactiveVector3()
            {
            }
            
            public ReactiveVector3(string dataName, Vector3 initialValue) : base(dataName, initialValue)
            {
            }
      }
}
```

## Dependency Validation

To ensure that a `DataAssetSo` is correctly configured for a specific consumer, you can use the `[RequireDataKeys]` attribute. It verifies at edit-time that the referenced asset contains all the specified keys, displaying a clear warning in the inspector if any are missing.

For example, if this `[RequireDataKeys]` is set with a `PlayerScore` and a `PlayerHealth`

```csharp
using DataAsset.Core;
using DataAsset.Core.Attributes;
using UnityEngine;

public sealed class PlayerManager : MonoBehaviour
{
      [RequireDataKeys("PlayerScore", "PlayerHealth")]
      public DataAssetSo playerData;

      private void OnEnable()
      {
            if (playerData == null)
            {
                  Debug.LogError("Player data asset is not assigned.", this);

                  return;
            }

            playerData.OnAnyDataChanged += HandlePlayerDataChanged;
      }

      private void OnDisable()
      {
            if (playerData != null)
            {
                  playerData.OnAnyDataChanged -= HandlePlayerDataChanged;
            }
      }

      private void HandlePlayerDataChanged(DataObject data)
      {
            // Handle the player data change event here
      }
}
```

This will generate this warning :

![Screenshot_5](https://github.com/user-attachments/assets/6e0019f9-0678-4ee9-b1dd-07812c5ac56f)

Until this is implemented :

![Scrrenshot_6](https://github.com/user-attachments/assets/27ba9265-9c2a-4a4e-80dd-78d75f8483fb)

## Automatic Key Generation

To avoid errors from string-based lookups ("magic strings"), the system includes a tool to generate a static C# class from your `DataAssetSo`. This class contains all your data keys as `const string` fields, providing type-safe access and enabling IDE auto-completion.
This is located under `Tools/Data Asset/Generate All Keys`.

```csharp
using DataAsset.Base.Primitive;
using DataAsset.Core;
using UnityEngine;

public sealed class PlayerManager : MonoBehaviour
{
      public DataAssetSo playerData;
      private ReactiveFloat playerHealth;

      private void OnEnable()
      {
            // Magic strings
            playerHealth = playerData.GetData<ReactiveFloat>("PlayerHealth");

            // Using generated keys
            playerHealth = playerData.GetData<ReactiveFloat>(Player_Keys.PlayerHealth);
      }
}
```

## Usage Scanning

The editor includes a powerful utility to find every usage of a data key across your entire project. It scans scripts, prefabs, and scenes to build a comprehensive list of references. You can click on any entry in the list to navigate directly to the corresponding location in your project. This is invaluable for debugging and safe refactoring.

For example :

![Screenshot_7](https://github.com/user-attachments/assets/f22263d0-4527-466a-9b62-ab98f08c6ab3)

Scan results are cached in the Editor. It is recommended to re-run the scan if you make significant changes to your scripts to ensure the usage data is up-to-date.

## Requirements

-   Unity 2021.3 LTS or newer.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
