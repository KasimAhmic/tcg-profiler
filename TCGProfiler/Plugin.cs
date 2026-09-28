using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TCGProfiler;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static TcgProfiler profiler;

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Patches.Apply();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ReferenceEquals(profiler, null) && profiler)
        {
            Logger.LogInfo("TCGProfiler already exists, skipping creation");
            return;
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loading...");

        var gameObject = new GameObject("TCGProfiler");
        DontDestroyOnLoad(gameObject);
        profiler = gameObject.AddComponent<TcgProfiler>();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded!");
    }
}