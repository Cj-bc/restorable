using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using UnityEngine;


[CreateAssetMenu(menuName = "Restorable/Store", fileName = "RestorableStore")]
public class RestorableStore : ScriptableObject
{
    public const string RESTORABLE_VERSION = "0.0.1";
    [SerializeField] private string _configPath = "";

    private Dictionary<string, Restorable> _registeredItems = new();

    public void Register(Restorable restorable)
    {
        _registeredItems.Add(restorable.GetType().FullName, restorable);
    }

    [ContextMenu("Dump")]
    public void Save()
    {
        JsonObject root = new JsonObject
        {
            ["restorable"] = new JsonObject
            {
                ["version"] = RESTORABLE_VERSION,
                ["registeredItems"] = new JsonArray(
                ),
            }
        };

        JsonArray itemsRoot = (root!["restorable"]!["registeredItems"] as JsonArray);
        foreach (var item in _registeredItems)
        {
            itemsRoot.Add(new JsonObject
            {
                ["key"] = item.Key,
                ["snapshot"] = item.Value.MakeSnapshot(),
            });
        }
        
        File.WriteAllText(_configPath, root.ToJsonString(new() { WriteIndented = true }));
    }

    [ContextMenu("Restore")]
    public void Restore()
    {
        if (!File.Exists(_configPath))
        {
            Debug.LogWarning($"[RestorableStore] Config file not found at {_configPath}");
            return;
        }
        using Stream stream = File.OpenRead(_configPath);

        var doc = JsonNode.Parse(stream);

        if (doc?["restorable"] is not JsonObject root)
        {
            Debug.LogWarning("[RestorableStore] invalid json format: Root not found.");
            return;
        }

        if (root?["version"] is not JsonValue ver
            || !ver.TryGetValue<string>(out var version)
            || version != RESTORABLE_VERSION)
        {
            Debug.LogWarning("[RestorableStore] version mismatch");
            return;
        }

        if (root?["registeredItems"] is JsonArray items)
        {
            foreach (var item in items)
            {
                if (item?["key"] is JsonValue keyNode
                    && item?["snapshot"] is JsonObject snapshot
                    && keyNode.TryGetValue<string>(out string key)
                    && _registeredItems.ContainsKey(key)
                    && _registeredItems[key] is Restorable restorable)
                {
                    restorable.Restore(snapshot);
                }
            }
        }
    }
}
