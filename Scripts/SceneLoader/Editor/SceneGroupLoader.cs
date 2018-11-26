using System;
using System.Collections.Generic;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class SceneGroupLoader : OdinEditorWindow
{
    [Serializable]
    private struct Element
    {
        [ShowInInspector, HorizontalGroup, HideLabel, ReadOnly]
        private SceneGroup _sceneGroup;
        
        public Element(SceneGroup sceneGroup)
        {
            _sceneGroup = sceneGroup;
        }

        [Button, HorizontalGroup]
        private void Load()
        {
            _sceneGroup.Load();
        }
    }

    [ShowInInspector, ListDrawerSettings(IsReadOnly = true), DisableContextMenu]
    private Element[] _sceneGroups = new Element[0];

    [MenuItem("Tools/SceneGroup Loader #_`")]
    private static void OpenWindow()
    {
        GetWindow<SceneGroupLoader>().Show();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        List<SceneGroup> assets = FindAssetsByType<SceneGroup>();
        _sceneGroups = new Element[assets.Count];
        for (int i = 0; i < assets.Count; i++)
            _sceneGroups[i] = new Element(assets[i]);
    }

    private static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
} 