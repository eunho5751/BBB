using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[Serializable]
public struct SceneInfo
{
    [SerializeField, ValueDropdown("ScenePathes_Editor")]
    private string _scene;

#if UNITY_EDITOR
    private static string[] ScenePathes_Editor
    {
        get
        {
            int count = SceneManager.sceneCountInBuildSettings;
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
                names[i] = SceneUtility.GetScenePathByBuildIndex(i);
            return names;
        }
    }
#endif

    public string ScenePath => _scene;
}

[CreateAssetMenu]
public class SceneGroup : ScriptableObject
{
    public int _id;
    [SerializeField, ValueDropdown("ScenePathes_Editor")]
    private string _activeScene;
    [SerializeField]
    private SceneInfo[] _scenes = new SceneInfo[0];

    private event Action<AsyncOperation> _onSceneLoad;
    private Scene[] _originalScenes;
    private int _loadedCount;

#if UNITY_EDITOR
    private string[] ScenePathes_Editor => Array.ConvertAll(_scenes, s => s.ScenePath);
#endif

    [Button(ButtonSizes.Medium)]
    public void Load()
    {
        if (_scenes.Length == 0)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            LoadEditorInternal();
            return;
        }
#endif
    }

#if UNITY_EDITOR
    private void LoadEditorInternal()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            if (SceneCount > 0)
            {
                // Open the first scene as OpenSceneMode.Single to unload other scenes.
                EditorSceneManager.OpenScene(_scenes[0].ScenePath, OpenSceneMode.Single);

                for (int i = 1; i < SceneCount; i++)
                {
                    EditorSceneManager.OpenScene(_scenes[i].ScenePath, OpenSceneMode.Additive);
                }

                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath(_activeScene));
            }
        }
    }
#endif

    public int SceneCount => _scenes.Length;
    public string ActiveScene => _activeScene;
    public SceneInfo[] SceneInfoList => _scenes;
}
