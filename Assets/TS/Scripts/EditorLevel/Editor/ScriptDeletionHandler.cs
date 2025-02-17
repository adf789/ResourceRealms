using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ScriptDeletionHandler : AssetModificationProcessor
{
    public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
    {
        if (assetPath.EndsWith(".cs"))
        {
            string scriptName = Path.GetFileNameWithoutExtension(assetPath);
            RemoveScriptFromPrefabs(scriptName);
        }

        return AssetDeleteResult.DidNotDelete;
    }

    private static void RemoveScriptFromPrefabs(string scriptName)
    {
        // ��� ������ ã��
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null) continue;

            bool modified = false;
            var components = prefab.GetComponentsInChildren<Component>(true);

            foreach (var component in components)
            {
                if (!component && component.GetType().Name == scriptName)
                {
                    Debug.Log($"[ScriptDeletionHandler] '{prefab.name}' �����տ��� ������ ��ũ��Ʈ �߰�, ���� ��...");
                    GameObject target = component.gameObject;
                    var serializedObject = new SerializedObject(target);
                    var property = serializedObject.FindProperty("m_Component");

                    for (int i = property.arraySize - 1; i >= 0; i--)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        var objRef = element.objectReferenceValue;
                        if (objRef == null)
                        {
                            property.DeleteArrayElementAtIndex(i);
                            modified = true;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (modified)
            {
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log($"[ScriptDeletionHandler] '{prefab.name}' �����տ��� ������ ��ũ��Ʈ�� �����ϰ� �����߽��ϴ�.");
            }
        }
    }
}
