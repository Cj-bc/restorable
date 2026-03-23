# Restorable

[日本語版はこちら / Japanese](./README_JA.md)

A Unity package that provides a simple snapshot-based save/restore system for game state.

## Overview

**Restorable** is a Unity package (`com.github.cj-bc.unity.restorable`) that lets you save and restore the state of your objects using JSON snapshots.

It consists of two main components:

- **`Restorable` interface** — Implement this on any class whose state you want to save. It requires two methods:
  - `MakeSnapshot()` — Serializes the current state into a `JsonObject`.
  - `Restore(JsonObject snapshot)` — Restores state from a previously saved `JsonObject`.

- **`RestorableStore` ScriptableObject** — Manages a collection of `Restorable` objects and handles reading/writing snapshots to a JSON file on disk.

## Usage

### 1. Implement the `Restorable` Interface

```csharp
using System.Text.Json.Nodes;

public class MyGameState : Restorable
{
    private int _score;
    private string _playerName;

    public JsonObject MakeSnapshot()
    {
        return new JsonObject
        {
            ["score"] = _score,
            ["playerName"] = _playerName,
        };
    }

    public void Restore(JsonObject snapshot)
    {
        _score = snapshot["score"]?.GetValue<int>() ?? 0;
        _playerName = snapshot["playerName"]?.GetValue<string>() ?? "";
    }
}
```

### 2. Create a `RestorableStore` Asset

In the Unity Editor, right-click in the Project window and select:

```
Create > Restorable > Store
```

Configure the store in the Inspector:

| Field | Description |
|-------|-------------|
| **Raw Config Path** | Path to the save file (filename or relative path) |
| **Path Root** | Base directory for the save file: `Absolute`, `PersistentDataPath`, or `DataPath` |

### 3. Register Objects and Save/Restore

```csharp
[SerializeField] private RestorableStore _store;

void Start()
{
    // Register objects that should be saved/restored
    _store.Register(myGameState);
}

void SaveGame()
{
    _store.Save();
}

void LoadGame()
{
    _store.Restore();
}
```

You can also trigger **Save** and **Restore** directly from the Inspector via the context menu on the `RestorableStore` component.

## Installation

Add the package via the Unity Package Manager using the Git URL of this repository, or copy the `Packages/com.github.cj-bc.unity.restorable` directory into your project's `Packages` folder.

**Dependencies:**
- `org.nuget.system.text.json` 10.0.2 — must be installed via [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)
