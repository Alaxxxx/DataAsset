# Unity DataAsset

This project provides a framework for implementing a data-oriented design in Unity. It uses `ScriptableObjects` as a robust alternative to the Singleton pattern for managing shared state and data, ensuring a clear separation between data and logic.

## Core Concepts

-   **`DataAsset`**: A `ScriptableObject` that acts as a container for a list of data objects.
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
-   **Dependency Validation**: Use the `[RequireDataKeys]` attribute on a `DataAsset` field. The inspector will warn you at edit-time if the referenced asset is missing any of the specified keys.
-   **Automatic Key Generation**: To avoid error-prone "magic strings", the editor includes a tool to generate a static C# class containing all your data keys as `const string`. This ensures type-safe access in your code.

## How to Use

### 1. Create a DataAsset

Create an asset via the **Assets > Create > ScriptableObjects > DataAsset** menu.

### 2. Add Data Objects

Select the `DataAsset` file to open the custom inspector. Use the "Add New Data" section to add `DataObject` entries.

![Screenshot_1](https://github.com/user-attachments/assets/48a95d47-8b53-41fe-b608-e904df877dbd)

### 3. Access Data in a Script

Reference the `DataAsset` in a component and use `GetData<T>()` to access data.

```csharp
using UnityEngine;
using DataAsset.Core;
using DataAsset.Base.Primitive;

public class Player : MonoBehaviour
{
    public DataAsset.Core.DataAsset playerStatsAsset;
    private ReactiveInt playerHealth;

    void Start()
    {
        playerHealth = playerStatsAsset.GetData<ReactiveInt>("PlayerHealth");

        if (playerHealth != null)
        {
            playerHealth.OnValueChanged += HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(int newHealth)
    {
        Debug.Log($"Health changed! New health: {newHealth}");
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnValueChanged -= HandleHealthChanged;
        }
    }
}
```

## Extending the System

To create a custom data type, simply create a new class that inherits from `ReactiveValue<T>` or `ReactiveList<T>`. The system will automatically detect and integrate it.

```csharp
using System;
using UnityEngine;
using DataAsset.Core;

[Serializable]
public class ReactiveVector3 : ReactiveValue<Vector3>
{
    public ReactiveVector3() : base() { }
    public ReactiveVector3(string dataName, Vector3 initialValue) : base(dataName, initialValue) { }
}
```

## Dependency Validation

To ensure that a `DataAsset` is correctly configured for a specific consumer, you can use the `[RequireDataKeys]` attribute. It verifies at edit-time that the referenced asset contains all the specified keys, displaying a clear warning in the inspector if any are missing.

```csharp
using UnityEngine;
using DataAsset.Core;
using DataAsset.Core.Attributes; // Required namespace

public class UIManager : MonoBehaviour
{
    // The inspector will show a warning if this DataAsset is missing
    // a "PlayerHealth" or "PlayerScore" key.
    [SerializeField]
    [RequireDataKeys("PlayerHealth", "PlayerScore")]
    private DataAsset.Core.DataAsset playerStatsAsset;
    
    // ...
}
```

## Automatic Key Generation

To avoid errors from string-based lookups ("magic strings"), the system includes a tool to generate a static C# class from your `DataAsset`. This class contains all your data keys as `const string` fields, providing type-safe access and enabling IDE auto-completion.

**Before:**
```csharp
// Prone to typos, no auto-completion
playerHealth = playerStatsAsset.GetData<ReactiveInt>("PlayerHealth");
```

**After:**
```csharp
// Safe, with auto-completion
playerHealth = playerStatsAsset.GetData<ReactiveInt>(PlayerStats_Keys.PlayerHealth);
```

## Usage Scanning

The editor includes a powerful utility to find every usage of a data key across your entire project. It scans scripts, prefabs, and scenes to build a comprehensive list of references. You can click on any entry in the list to navigate directly to the corresponding location in your project. This is invaluable for debugging and safe refactoring.

## Requirements

-   Unity 2021.3 LTS or newer.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
