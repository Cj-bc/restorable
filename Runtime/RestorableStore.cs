using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using UnityEngine;


[CreateAssetMenu(menuName = "Restorable/Store", fileName = "RestorableStore")]
public class RestorableStore : ScriptableObject
{
    public const string RESTORABLE_VERSION = "0.0.1";

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
        
        Debug.Log(root.ToJsonString(new () { WriteIndented = true } ));
    }

    public void Restore()
    {
    }
}
