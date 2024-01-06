using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GameplayUIReducer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; set; }
    internal static readonly Dictionary<string, ConfigEntry<bool>> ConfigEntries = new();
    internal static readonly Dictionary<string, string> ElementPaths = new();

    private void Awake()
    {
        Log = Logger;

        DeclareElements();
        SetupConfig();
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void SetupConfig()
    {
        foreach (var elementPath in ElementPaths)
        {
            ConfigEntries.Add(elementPath.Key,
                Config.Bind("Element Hiding", elementPath.Key, false,
                    $"Hides the {elementPath.Key.ToLower()} element.")
            );
        }

        object ttSettings = OptionalTootTallySettings.AddNewPage("Gameplay UI Reducer", "Gameplay UI Reducer", 40, new Color(.1f, .1f, .1f, .1f));
        if (ttSettings == null) return;
        foreach (var configEntry in ConfigEntries)
        {
            OptionalTootTallySettings.AddToggle(ttSettings, $"Hide {configEntry.Key}", configEntry.Value);
        }
    }

    private void DeclareElements()
    {
        ElementPaths.Add("Breath Meter", "BreathCanvas");
        ElementPaths.Add("Champ Text", "ChampCanvas");
        ElementPaths.Add("Health Meter", "HealthMask");
        ElementPaths.Add("Rainbow Borders", null);
        ElementPaths.Add("Longest Combo", "GameplayCanvas/UIHolder/maxcombo");
        ElementPaths.Add("Left Bounds", "GameplayCanvas/GameSpace/LeftBounds");
        ElementPaths.Add("Note Lines", "GameplayCanvas/GameSpace/NoteLinesHolder");
        ElementPaths.Add("Notes", "GameplayCanvas/GameSpace/NotesHolder");
        ElementPaths.Add("Lyrics", "GameplayCanvas/GameSpace/NotesHolder/AllLyrics");
        ElementPaths.Add("Note Explosions", "GameplayCanvas/GameSpace/NoteEndExplosions");
        ElementPaths.Add("Note Cursor", "GameplayCanvas/GameSpace/TargetNote");
        ElementPaths.Add("Multiplier Popup", "GameplayCanvas/Popups/popup_mult");
        ElementPaths.Add("Accuracy Popup", "GameplayCanvas/Popups/popup_text");
        ElementPaths.Add("No Gap Popup", "GameplayCanvas/Popups/no_gap");
        ElementPaths.Add("Max Popup", "GameplayCanvas/Popups/MAX");
        ElementPaths.Add("Song Name", "GameplayCanvas/UIHolder/upper_right/Song Name Shadow");
        ElementPaths.Add("Score Counter", "GameplayCanvas/UIHolder/upper_right/ScoreShadow");
        ElementPaths.Add("Time Elapsed", "GameplayCanvas/UIHolder/time_elapsed");
        ElementPaths.Add("Time Elapsed Progress Bar", "GameplayCanvas/UIHolder/time_elapsed_bar");
        ElementPaths.Add("Tromboner Model", "PlayerModelHolder");
    }

    internal static bool IsRainbowVisible() => !ConfigEntries["Rainbow Borders"]?.Value ?? true;
}

[HarmonyPatch(typeof(GameController), "Start")]
internal class GameControllerStartPatch
{
    static void Postfix()
    {
        foreach (var elementPath in Plugin.ElementPaths)
        {
            bool isHidden = Plugin.ConfigEntries[elementPath.Key]?.Value ?? false;
            if (isHidden && !string.IsNullOrWhiteSpace(elementPath.Value))
            {
                GameObject gameObject = GameObject.Find(elementPath.Value);
                if (gameObject == null)
                {
                    Plugin.Log.LogError($"Unable to find {elementPath.Key} at '{elementPath.Value}', it will not be hidden.");
                }
                else
                {
                    Plugin.Log.LogDebug($"Hiding element {elementPath.Key}...");
                    gameObject.SetActive(false);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(RainbowEffect), nameof(RainbowEffect.startRainbowLess))]
internal class RainbowEffectStartPatch
{
    static bool Prefix(ref bool ___champmode)
    {
        ___champmode = true;
        return Plugin.IsRainbowVisible();
    }
}

[HarmonyPatch(typeof(RainbowEffect), nameof(RainbowEffect.stopRainbow))]
internal class RainbowEffectStopPatch
{
    static bool Prefix(ref bool ___champmode)
    {
        ___champmode = false;
        return Plugin.IsRainbowVisible();
    }
}
