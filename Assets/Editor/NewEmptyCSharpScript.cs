using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreatePrefabFromSelection
{
    [MenuItem("Tools/Create Prefab From Selection")]
    private static void CreatePrefab()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Select a GameObject in the Hierarchy first.");
            return;
        }

        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
            AssetDatabase.Refresh();
        }

        string path = "Assets/Prefabs/" + selected.name + ".prefab";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        PrefabUtility.SaveAsPrefabAsset(selected, path, out bool success);

        if (success)
            Debug.Log($"Prefab created at {path}");
        else
            Debug.LogError("Failed to create prefab.");
    }
}