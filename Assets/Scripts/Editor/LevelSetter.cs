using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSetter : EditorWindow
{
    private  LevelLibrary library;
    [MenuItem("Tools/CurrentLevelSetter")]
    public static void ShowLevelPopup()
    {
        LevelSetter wnd = GetWindow<LevelSetter>();
        wnd.titleContent = new GUIContent("Set Current Level");
        
    }

    private void Awake()
    {
        var assets = AssetDatabase.FindAssets("Level Library", new[] { "Assets/ScriptableObjects" });
        var assetName = assets[0];
        var path = AssetDatabase.GUIDToAssetPath(assetName);
        library = AssetDatabase.LoadAssetAtPath<LevelLibrary>(path);
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        Label label = new Label($"Current Level: {PlayerPrefs.GetInt("LevelIndex",0) +1}");
        root.Add(label);
        
        Label label2 = new Label($"Max  Level: {library.Levels.Count}");
        root.Add(label2);
        
        TextInputBaseField<int> inputField = new IntegerField($"Level");
        inputField.value = (PlayerPrefs.GetInt("LevelIndex", 0) + 1);
        inputField.name = "Level Input";
        root.Add(inputField);
        
        Button button = new Button();
        button.text = "Set Level";
        button.clicked += OnButtonClicked;
        root.Add(button);
    }

    private void OnButtonClicked()
    {
        int typedLevel = 0;

        var inputfield = rootVisualElement[2] as TextInputBaseField<int>;
        typedLevel = inputfield.value;
        
        PlayerPrefs.SetInt("LevelIndex",typedLevel-1);
        var label = rootVisualElement[0] as Label;
        label.text = $"Current Level: {PlayerPrefs.GetInt("LevelIndex", 0) + 1}";
    }
}
