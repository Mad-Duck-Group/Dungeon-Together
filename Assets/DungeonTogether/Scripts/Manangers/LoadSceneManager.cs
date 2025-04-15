using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Redcode.Extensions;
using TriInspector;
using Unity.Netcode;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    MainMenu,
    Lounge,
    Game
}

public class LoadSceneManager : PersistentMonoSingleton<LoadSceneManager>
{
    [SerializedDictionary("SceneType", "Scene")]
    public SerializedDictionary<SceneType, SceneReference> sceneAssets;
    
    public static SceneType CurrentSceneType { get; private set; }

    private void Start()
    {
        if (sceneAssets == null || sceneAssets.Count == 0)
        {
            Debug.LogError("Scene assets are not set up correctly.");
            return;
        }

        var activeScene = SceneManager.GetActiveScene().path;
        CurrentSceneType = sceneAssets
            .FirstOrDefault(x => x.Value.Path == activeScene).Key;
    }

    public void LoadScene(string sceneName)
    {
        if (!Enum.TryParse(sceneName, out SceneType parsedSceneType))
        {
            Debug.LogError($"Scene {sceneName} not found in scene assets.");
            return;
        }
        LoadScene(parsedSceneType);
    }
    
    private void LoadScene(SceneType sceneType)
    {
        if (sceneAssets.TryGetValue(sceneType, out var sceneReference))
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneReference.Path, LoadSceneMode.Single);
            CurrentSceneType = sceneType;
        }
        else
        {
            Debug.LogError($"Scene {sceneType} not found in scene assets.");
        }
    }
}
