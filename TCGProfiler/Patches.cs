using HarmonyLib;
using JetBrains.Annotations;

namespace TCGProfiler;

public static class Patches
{
    public static void Apply()
    {
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }

#if PATCH_GAME_LOG_SPAM
    /*
     * The game has a "bug" where it will spam the log with null reference exceptions if the PlayCardSet is null. This
     * patch prevents that from happening.
     */

    [HarmonyPatch(typeof(PlayCardSetUI), "Update")]
    private static class PlayCardSetUIUpdatePatch
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool Prefix(PlayCardSet ___m_PlayCardSet)
        {
            return ___m_PlayCardSet != null;
        }
    }

    [HarmonyPatch(typeof(PlayCardSetUI), "LateUpdate")]
    private static class PlayCardSetUILateUpdatePatch
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool Prefix(PlayCardSet ___m_PlayCardSet)
        {
            return ___m_PlayCardSet != null;
        }
    }
#endif
}