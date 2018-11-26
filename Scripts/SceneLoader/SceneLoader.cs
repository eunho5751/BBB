using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private Queue<AsyncOperation> _opQueue = new Queue<AsyncOperation>();
    private SceneGroup _sceneGroup;
    private string _loadingSceneName;
    private int _loadedCount;

    public SceneLoader(string loadingSceneName)
    {
        _loadingSceneName = loadingSceneName;
    }

    public void Load(SceneGroup group)
    {
        _sceneGroup = group;
        _loadedCount = 0;
        
        // Load Loading Scene
        var op = SceneManager.LoadSceneAsync(_loadingSceneName, LoadSceneMode.Single);
        CoroutineManager.Start(LoadingScene(op));
    }

    private IEnumerator LoadingScene(AsyncOperation op)
    {
        yield return op;

        // Start Loading
        foreach (var info in _sceneGroup.SceneInfoList)
        {
            var op2 = SceneManager.LoadSceneAsync(info.ScenePath, LoadSceneMode.Additive);
            CoroutineManager.Start(WaitLoading(op2));
        }
    }

    private IEnumerator WaitLoading(AsyncOperation op)
    {
        _opQueue.Enqueue(op);
        op.allowSceneActivation = false;
        
        while (op.progress < 0.9f)
            yield return null;

        _loadedCount++;
        if (_loadedCount == _sceneGroup.SceneCount)
        {
            while (_opQueue.Count > 0)
            {
                var op2 = _opQueue.Dequeue();
                op2.allowSceneActivation = true;
                yield return op2;
            }

            // Unload Loading Scene
            Unload();

            // Set New Active Scene
            Scene activeScene = SceneManager.GetSceneByPath(_sceneGroup.ActiveScene);
            if (activeScene.IsValid())
                SceneManager.SetActiveScene(activeScene);
        }
    }

    private void Unload()
    {
        Scene loadingScene = SceneManager.GetActiveScene();
        SceneManager.UnloadSceneAsync(loadingScene);
    }
}

