using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ScriptCreatorEditorWindow : EditorWindow
{
    private string basePath = "Assets/TS/Scripts/{0}/";
    private string basePrefabPath = "Assets/TS/ResourcesAddressable/Prefabs/";
    private string objectName = "";
    private List<string> objectAddPaths = null;

    [MenuItem("Tools/Create Script %e")] // Ctrl + E ����Ű ����
    public static void ShowWindow()
    {
        GetWindow<ScriptCreatorEditorWindow>("Script Creator");
    }

    private void OnEnable()
    {
        objectAddPaths = new List<string>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Script Creator", EditorStyles.boldLabel);

        if (objectAddPaths.Count > 0)
            GUILayout.Label("Addable Path");

        GUIStyle layoutStyle = new GUIStyle();
        layoutStyle.alignment = TextAnchor.MiddleLeft;
        layoutStyle.fixedWidth = CalculateTotalWidth(objectAddPaths);

        EditorGUILayout.BeginHorizontal(layoutStyle);
        {
            string slash = "/";

            int index = 0;

            foreach (string path in objectAddPaths)
            {
                // ��ư ���ڿ� ���̿� �´� �� ���
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(path));
                float buttonWidth = size.x + 10; // ���� ���� �߰�

                if (GUILayout.Button(path, GUILayout.Width(buttonWidth)))
                {
                    objectAddPaths.Remove(path);
                    break;
                }

                if(index < objectAddPaths.Count - 1)
                    GUILayout.Label(slash);

                index++;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        {
            objectName = EditorGUILayout.TextField("ObjectName", objectName);
        }
        bool isChangeName = EditorGUI.EndChangeCheck();

        if (isChangeName)
        {
            if(TryGetSeperatePath(ref objectName, out string path))
                objectAddPaths.Add(path);
        }

        if (GUILayout.Button("Generate MVC Structure"))
        {
            GenerateMVCStructure();
        }
    }

    private float CalculateTotalWidth(List<string> paths)
    {
        string slash = "/";
        float totalWidth = 0f;

        foreach (string path in paths)
        {
            Vector2 buttonSize = GUI.skin.label.CalcSize(new GUIContent(path));
            totalWidth += buttonSize.x + 10; // ��ư �ʺ� ���

            // �������� �� �߰�
            if (path != paths[^1]) // ������ �������� �ƴϸ� ������ �߰�
            {
                Vector2 slashSize = GUI.skin.label.CalcSize(new GUIContent(slash));
                totalWidth += slashSize.x;
            }
        }

        return totalWidth;
    }

    // ���ڿ� �� ��ΰ� �� �� �ִ� �κ��� ������ �и�.
    private bool TryGetSeperatePath(ref string name, out string extractedPath)
    {
        extractedPath = null;

        if (string.IsNullOrEmpty(name))
            return false;

        if (name.Length > 0 && name[0] == '/')
        {
            name = name.Substring(1, name.Length - 1);
        }
        else
        {
            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (name[i] == '/')
                {
                    extractedPath = name.Substring(0, i);

                    if (i + 1 < name.Length)
                        name = name.Substring(i + 1, name.Length - (i + 1));
                    else
                        name = string.Empty;

                    return true;
                }
            }
        }

        return false;
    }

    private void GenerateMVCStructure()
    {
        if (string.IsNullOrEmpty(objectName))
        {
            Debug.LogError("Object name cannot be empty.");
            return;
        }

        // ��ο��� `/`�� �������� ���� ���� ����
        string modelPath = string.Format(basePath, "LowLevel");
        string viewPath = string.Format(basePath, "MiddleLevel");
        string controllerPath = string.Format(basePath, "HighLevel");
        string prefabPath = basePrefabPath;

        if (objectAddPaths.Count > 0)
        {
            string addPath = $"{string.Join('/', objectAddPaths)}/";

            modelPath = Path.Combine(modelPath, addPath);
            viewPath = Path.Combine(viewPath, addPath);
            controllerPath = Path.Combine(controllerPath, addPath);
            prefabPath = Path.Combine(prefabPath, addPath);
        }

        CreateDirectoryIfNotExist(modelPath);
        CreateDirectoryIfNotExist(viewPath);
        CreateDirectoryIfNotExist(controllerPath);
        CreateDirectoryIfNotExist(prefabPath);

        CreateScript(modelPath, $"{objectName}Model", GenerateModelCode(objectName));
        CreateScript(viewPath, $"{objectName}View", GenerateViewCode(objectName));
        CreateScript(controllerPath, $"{objectName}Controller", GenerateControllerCode(objectName));

        CreatePrefab(prefabPath, $"{objectName}Prefab");

        AssetDatabase.Refresh();
        Debug.Log("MVC structure created successfully!");
    }

    private void CreateDirectoryIfNotExist(string path)
    {
        // ��� ���� ���丮 �����Ͽ� ����
        string normalizedPath = path.Replace("\\", "/");
        if (!Directory.Exists(normalizedPath))
        {
            Directory.CreateDirectory(normalizedPath);
        }
    }

    private void CreateScript(string path, string fileName, string content)
    {
        string filePath = Path.Combine(path, $"{fileName.Replace("/", "")}.cs").Replace("\\", "/");
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, content);
        }
    }

    private string GenerateModelCode(string name)
    {
        return $@"
public class {name}Model : BaseModel
{{
    
}}";
    }

    private string GenerateViewCode(string name)
    {
        return $@"
using UnityEngine;

public class {name}View : BaseView<{name}Model>
{{

}}";
    }

    private string GenerateControllerCode(string name)
    {
        return $@"
using UnityEngine;

public class {name}Controller
{{
    private {name}Model model;
    private {name}View view;
}}";
    }

    private void CreatePrefab(string path, string prefabName)
    {
        string prefabFilePath = Path.Combine(path, $"{prefabName}.prefab").Replace("\\", "/");
        if (!File.Exists(prefabFilePath))
        {
            GameObject obj = new GameObject(prefabName);
            obj.AddComponent<Transform>();
            PrefabUtility.SaveAsPrefabAsset(obj, prefabFilePath);
            DestroyImmediate(obj);
        }
    }
}
