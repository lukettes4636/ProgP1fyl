using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class ListUnusedScripts
{
    [MenuItem("Tools/List Unused Scripts")] 
    static void ListUnused()
    {
        var scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] {"Assets/Scripts"});
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] {"Assets"});
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
        var assetPaths = new List<string>();
        for (int i = 0; i < sceneGuids.Length; i++) assetPaths.Add(AssetDatabase.GUIDToAssetPath(sceneGuids[i]));
        for (int i = 0; i < prefabGuids.Length; i++) assetPaths.Add(AssetDatabase.GUIDToAssetPath(prefabGuids[i]));
        var unused = new List<string>();
        for (int i = 0; i < scriptGuids.Length; i++)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuids[i]);
            var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            var type = ms != null ? ms.GetClass() : null;
            if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                continue;
            }
            string guid = AssetDatabase.AssetPathToGUID(scriptPath);
            bool used = false;
            for (int j = 0; j < assetPaths.Count; j++)
            {
                string p = assetPaths[j];
                if (!File.Exists(p)) continue;
                string t = File.ReadAllText(p);
                if (t.Contains(guid)) { used = true; break; }
            }
            if (!used) unused.Add(scriptPath);
        }
        if (unused.Count == 0)
        {
            Debug.Log("No unused scripts found");
        }
        else
        {
            Debug.Log("Unused scripts: " + unused.Count);
            for (int i = 0; i < unused.Count; i++) Debug.Log(unused[i]);
        }
    }
}
