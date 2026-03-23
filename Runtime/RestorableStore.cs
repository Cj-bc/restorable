using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using UnityEngine;


[CreateAssetMenu(menuName = "Restorable/Store", fileName = "RestorableStore")]
public class RestorableStore : ScriptableObject
{
    public enum PathRoot
    {
        Absolute, PersistentDataPath, DataPath
    }
    public const string RESTORABLE_VERSION = "0.0.1";
    [SerializeField] private string _rawConfigPath = "";
    [SerializeField] private PathRoot _pathRoot;
    private string _configPath => _pathRoot switch
    {
        PathRoot.Absolute => _rawConfigPath,
        PathRoot.PersistentDataPath => Path.Combine(Application.persistentDataPath, _rawConfigPath),
        PathRoot.DataPath => Path.Combine(Application.dataPath, _rawConfigPath),
    };

    private Dictionary<string, Restorable> _registeredItems = new();

    public void Register(Restorable restorable)
    {
        _registeredItems.Add(restorable.GetType().FullName, restorable);
    }

    [ContextMenu("Save")]
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
        
        EnsureFileExists(_configPath);
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

        /*
          読み込みだけであれば JsonDocument の方が速くimmutableであるが、

          + JsonNode の方が取り回しやすい(APIの書き心地が良い)
          + 頻繁に呼び出される関数ではなく、そこまでパフォーマンスを気にする必要がない

          ことから JsonNode で書いている。
        */
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

    private static void EnsureFileExists(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        if (Directory.Exists(path))
        {
            Debug.LogError($"[RestorableStore] Invalid config path: '{path}' is a directory.");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using var st = File.Create(path);
    }
}
