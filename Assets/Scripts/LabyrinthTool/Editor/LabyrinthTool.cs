using UnityEngine;
using UnityEditor;
using System;

public class LabyrinthTool : EditorWindow
{
    public ScriptableLabyrinthInformations scriptableLabyrinthInformation;
    public ScriptableRoomTiles scriptableRoomTiles;
    public ScriptableObjectSpawnForRoom scriptableRoomSpawn;

    bool isOpenSO1;
    bool isOpenSO2;
    bool isOpenSO3;

    Editor editor;

    private Vector2 scrollPos;

    Vector3 centerPosition = Vector3.zero;

    [MenuItem("Tools/LabyrinthGenerator")]
    public static void ShowWindow()
    {
        GetWindow<LabyrinthTool>("Labyrinth Generator");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        //Header
        GUILayout.Label("Labyrinth Generator", EditorStyles.whiteLargeLabel);
        GUILayout.Space(30f);

        DrawLine();

        centerPosition = EditorGUILayout.Vector3Field("Center Position: ", centerPosition);
        GUILayout.Space(10f);

        //ScriptableObjects
        DrawLine();

        SerializedObject tool = new SerializedObject(this);

        SetScriptableObjectField(ref tool, "scriptableLabyrinthInformation", typeof(ScriptableLabyrinthInformations), ref isOpenSO1, "Labyrinth Information: ");
        GUILayout.Space(10f);

        DrawLine();

        SetScriptableObjectField(ref tool, "scriptableRoomTiles", typeof(ScriptableRoomTiles), ref isOpenSO2, "Roomtiles Information: ");
        GUILayout.Space(10f);

        DrawLine();

        SetScriptableObjectField(ref tool, "scriptableRoomSpawn", typeof(ScriptableObjectSpawnForRoom), ref isOpenSO3, "Object Spawn Information: ");
        GUILayout.Space(10f);

        DrawLine();


        // Buttons
        GUILayout.Space(30f);
        if (GUILayout.Button("Generate"))
        {
            GenerateMaze();
        }
        GUILayout.Space(30f);

        EditorGUILayout.EndScrollView();

        tool.ApplyModifiedProperties();

    }

    private void GenerateMaze()
    {
        if (scriptableLabyrinthInformation == null)
        {
            Debug.LogError("Missing Labyrinth Information!");
            return;
        }
        else if(scriptableRoomTiles == null)
        {
            Debug.LogError("Missing Roomtile Information!");
            return;
        }
         else if (scriptableRoomSpawn == null)
        {
            Debug.LogError("Missing Object Spawn Information!");
            return;
        }


        LabyrinthController labyrinthController = new LabyrinthController();
        labyrinthController.Build(scriptableLabyrinthInformation, scriptableRoomTiles, scriptableRoomSpawn, centerPosition);
    }

    ScriptableObject SetScriptableObjectField(ref SerializedObject tool, string propertyPathName, Type type, ref bool isOpen, string label)
    {

        ScriptableObject createdScriptableObject = null;

        SerializedObject serializedObject = new SerializedObject(this);

        SerializedProperty serializedProperty = tool.FindProperty(propertyPathName);

        if (serializedProperty.objectReferenceValue==null)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(serializedProperty, new GUIContent(label));

            if (GUILayout.Button("Create"))
            {
                ScriptableObject newSO = CreateSO(type);

                createdScriptableObject = newSO;
                serializedProperty.objectReferenceValue = createdScriptableObject;
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.PropertyField(serializedProperty, new GUIContent(label));
            DrawScriptableEditor(serializedProperty.objectReferenceValue, null, ref isOpen, ref editor, "Inspect: ");
        }

        return createdScriptableObject;
    }


    // This Method is not my own, i got it from Gerald
    private void DrawScriptableEditor(UnityEngine.Object myObject, System.Action onObjectUpdated, ref bool foldout, ref Editor editorTodraw, string label)
    {
        if (myObject == null) return;

        foldout = EditorGUILayout.Foldout(foldout, label);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (!foldout) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(15, 0, 0, 0);
            EditorGUILayout.BeginVertical(boxStyle);

            Editor.CreateCachedEditor(myObject, null, ref editorTodraw);

            editorTodraw.OnInspectorGUI();

            EditorGUILayout.EndVertical();

            if (check.changed)
                onObjectUpdated?.Invoke();
        }
    }


    public static ScriptableObject CreateSO(Type ofType)
    {
        ScriptableObject newScriptableObject = ScriptableObject.CreateInstance(ofType);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{ofType.Name}.asset");
        Debug.Log($"Asset was created at Assets/{ofType.Name}.asset");

        AssetDatabase.CreateAsset(newScriptableObject, assetPath );
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = newScriptableObject;

        return newScriptableObject;
    }


    void DrawLine()
    {
        GUIStyle lineStyle = new GUIStyle();
        lineStyle.normal.background = EditorGUIUtility.whiteTexture;
        lineStyle.margin = new RectOffset(0, 0, 5, 5);
        lineStyle.fixedHeight = 0.3f;
        EditorGUILayout.LabelField(GUIContent.none, lineStyle);
    }
}



