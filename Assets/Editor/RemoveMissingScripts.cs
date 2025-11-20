using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class RemoveMissingScripts
{
    [MenuItem("Tools/Remove Missing Scripts In Scene")]
    static void RemoveInScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int total = 0;
        for (int i = 0; i < roots.Length; i++)
        {
            total += Clean(roots[i]);
        }
        if (total > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log("Removed missing scripts: " + total);
    }

    static int Clean(GameObject go)
    {
        int c = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        for (int i = 0; i < go.transform.childCount; i++)
        {
            c += Clean(go.transform.GetChild(i).gameObject);
        }
        return c;
    }

    [MenuItem("Tools/Remove Missing Scripts In Prefabs")]
    static void RemoveInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int total = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var root = PrefabUtility.LoadPrefabContents(path);
            int count = Clean(root);
            if (count > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                total += count;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log("Removed missing scripts from prefabs: " + total);
    }
}
