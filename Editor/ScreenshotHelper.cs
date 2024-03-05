using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.IO;
using Unity.EditorCoroutines.Editor;


public class ScreenshotHelper : EditorWindow
{
    #region VARIABLES
    private GUIStyle headerStyle;
    private GUIStyle subHeading;
    private GUIStyle smallBoldHeading;
    private List<string> resolutionNames = new List<string>();
    private List<string> filePaths = new List<string>();
    private List<Vector2Int> resolutions = new List<Vector2Int>();
    private ResolutionSettings resolutionSettings;
    private Vector2 scrollPosition = Vector2.zero;
    private List<bool> showSettings = new List<bool>();
    public enum GameViewSizeType
    {
        AspectRatio, FixedResolution
    }
    #endregion

    #region UNITY FUNCTIONS
    [MenuItem("Custom Helper/Screenshot Helper")]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotHelper>("Screenshot Helper");
    }
    private void OnEnable()
    {
        resolutionSettings = AssetDatabase.LoadAssetAtPath<ResolutionSettings>("Assets/ResolutionSettings.asset");
        if (resolutionSettings == null)
        {
            resolutionSettings = ScriptableObject.CreateInstance<ResolutionSettings>();
            AssetDatabase.CreateAsset(resolutionSettings, "Assets/ResolutionSettings.asset");
            AssetDatabase.SaveAssets();
        }
        for (int i = 0; i < resolutionSettings.resolutionNames.Count; i++)
        {
            showSettings.Add(false);
        }
    }
    private void OnGUI()
    {
        // Initialize the header style
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 20;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = Color.white;
        }
        if (smallBoldHeading == null)
        {
            smallBoldHeading = new GUIStyle(GUI.skin.label);
            smallBoldHeading.fontSize = 15;
            smallBoldHeading.fontStyle = FontStyle.Bold;
            smallBoldHeading.alignment = TextAnchor.MiddleCenter;
            smallBoldHeading.normal.textColor = Color.white;
        }
        if (subHeading == null)
        {
            subHeading = new GUIStyle(GUI.skin.label);
            subHeading.alignment = TextAnchor.MiddleCenter;
        }

        GUILayout.Label("Custom Screenshot Helper", headerStyle);
        GUILayout.Label("Created By Nehal V", subHeading);
        GUILayout.Label("Email :  nehalvxavier@gmail.com", subHeading);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


        GUILayout.Label("Resolution Settings", smallBoldHeading);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < resolutionSettings.resolutionNames.Count; i++)
        {
            string name = resolutionSettings.resolutionNames[i];
            if (name != "")
                name += "  - ";
            name += resolutionSettings.resolutions[i].x + " X " + resolutionSettings.resolutions[i].y;
            GUILayout.BeginHorizontal();
            showSettings[i] = EditorGUILayout.Foldout(showSettings[i], name);
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (GUILayout.Button("Capture", GUILayout.Width(60)))
            {
                //SetSize(2);
                TakeScreenshot(resolutionSettings.resolutions[i].x, resolutionSettings.resolutions[i].y, resolutionSettings.filePaths[i], resolutionSettings.resolutionNames[i], resolutionSettings.numberOfScreenshots[i],i);
                EditorUtility.SetDirty(resolutionSettings);
                AssetDatabase.SaveAssets();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            if (showSettings[i])
            {
                string newName = EditorGUILayout.TextField("Resolution Name", resolutionSettings.resolutionNames[i]);
                resolutionSettings.resolutionNames[i] = newName;

                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                GUILayout.Label("Save Path", GUILayout.MaxWidth(65));
                resolutionSettings.filePaths[i] = EditorGUILayout.TextField(resolutionSettings.filePaths[i]);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder Path", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        resolutionSettings.filePaths[i] = path;
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                EditorGUI.BeginDisabledGroup(true);

                resolutionSettings.resolutions[i] = EditorGUILayout.Vector2IntField("Resolution", resolutionSettings.resolutions[i]);

                EditorGUI.EndDisabledGroup();

                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                resolutionSettings.numberOfScreenshots[i] = EditorGUILayout.IntField("Take Number ", resolutionSettings.numberOfScreenshots[i]);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("See Files"))
                {
                    string folderPath = resolutionSettings.filePaths[i];
                    if (folderPath == "")
                    {
                        EditorUtility.DisplayDialog("Warning", "Folder Path is Empty", "Ok");
                    }
                    else
                    {
#if UNITY_EDITOR_WIN
        Process.Start("explorer.exe", "/select," + folderPath);
#else
                        EditorUtility.RevealInFinder(folderPath);
#endif
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

        }
        EditorGUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(Application.isPlaying);

        if (GUILayout.Button("Add Resolution"))
        {
            resolutionSettings.resolutionNames.Add("Screenshot");
            resolutionSettings.filePaths.Add(Application.persistentDataPath);

            Vector2 gameViewResolution = Handles.GetMainGameViewSize();

            resolutionSettings.resolutions.Add(new Vector2Int((int)gameViewResolution.x, (int)gameViewResolution.y));
            resolutionSettings.numberOfScreenshots.Add(0);
            showSettings.Add(true);

            EditorUtility.SetDirty(resolutionSettings);
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Remove Resolution"))
        {
            for (int i = 0; i < resolutionSettings.resolutions.Count; i++)
            {
                Vector2 gameViewResolution = Handles.GetMainGameViewSize();
                if(resolutionSettings.resolutions[i] == new Vector2Int((int)gameViewResolution.x, (int)gameViewResolution.y))
                {
                    resolutionSettings.resolutionNames.RemoveAt(i);
                    resolutionSettings.filePaths.RemoveAt(i);
                    resolutionSettings.resolutions.RemoveAt(i);
                    resolutionSettings.numberOfScreenshots.RemoveAt(i);
                    showSettings.RemoveAt(i);
                }
            }
        }
        if (GUILayout.Button("?", GUILayout.Width(40)))
        {
            EditorUtility.DisplayDialog("Message", "1. Choose required resolution on game view.Click on Add Resolution Button to add to list. \n 2. Select Game Resolution and Click on Remove Resolution to remove it from list.", "Ok");
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("Take Screenshot", GUILayout.Height(60)))
        {
            EditorCoroutineUtility.StartCoroutine(TakeAllScreenshots(), this);
        }
        EditorGUI.EndDisabledGroup();
    }
    #endregion

    #region FUNCTIONS

    IEnumerator TakeAllScreenshots()
    {
        for (int i = 0; i < resolutionSettings.resolutions.Count; i++)
        {
            if (resolutionSettings.filePaths[i] != "")
            {
                TakeScreenshot(resolutionSettings.resolutions[i].x, resolutionSettings.resolutions[i].y, resolutionSettings.filePaths[i], resolutionSettings.resolutionNames[i], resolutionSettings.numberOfScreenshots[i],i);

                yield return new WaitForEndOfFrame();
            }
        }
        EditorUtility.SetDirty(resolutionSettings);
        AssetDatabase.SaveAssets();
    }
    /*public static void SetSize(int index)
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        selectedSizeIndexProp.SetValue(gvWnd, index, null);
    }*/
    public static void SetSize(int index)
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        var SizeSelectionCallback = gvWndType.GetMethod("SizeSelectionCallback",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        SizeSelectionCallback.Invoke(gvWnd, new object[] { index, null });
    }
    int GetResolutionIndex(int width, int height)
    {
        Type gameViewSizes = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        Type generic = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizes);
        MethodInfo getGroup = gameViewSizes.GetMethod("GetGroup");
        object instance = generic.GetProperty("instance").GetValue(null, null);
        PropertyInfo currentGroupType = instance.GetType().GetProperty("currentGroupType");
        GameViewSizeGroupType groupType = (GameViewSizeGroupType)(int)currentGroupType.GetValue(instance, null);
        object group = getGroup.Invoke(instance, new object[] { (int)groupType });

        MethodInfo getBuiltinCount = group.GetType().GetMethod("GetBuiltinCount");
        MethodInfo getCustomCount = group.GetType().GetMethod("GetCustomCount");
        int totalResolutions = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);

        MethodInfo getGameViewSize = group.GetType().GetMethod("GetGameViewSize");

        for (int i = 0; i < totalResolutions; i++)
        {
            object gameViewSize = getGameViewSize.Invoke(group, new object[] { i });
            int resolutionWidth = (int)gameViewSize.GetType().GetProperty("width").GetValue(gameViewSize, null);
            int resolutionHeight = (int)gameViewSize.GetType().GetProperty("height").GetValue(gameViewSize, null);

            if (resolutionWidth == width && resolutionHeight == height)
            {
                return i;
            }
        }

        return -1; // Resolution not found
    }
    public void TakeScreenshot(int x,int y, string path,string fileName,int count, int c)
    {
        if(path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Output path is Empty. Cant Take Screenshot", "Ok");
            return;
        }
        int index = GetResolutionIndex(x, y);
        if (index !=-1)
        {
            SetSize(index);
            if (fileName == "")
                fileName = "Screenshot";
            string nameOfFile = fileName +"(" + x + "X" + y + ")_" + count+ ".png";
            string finalPath = Path.Combine(path, nameOfFile);
            ScreenCapture.CaptureScreenshot(finalPath);
            Debug.Log("Screenshot Saved : " + finalPath);
            resolutionSettings.numberOfScreenshots[c]++;
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "Unknown Issue. Cant Take Screenshot", "Ok");
        }
    }
    #endregion
}