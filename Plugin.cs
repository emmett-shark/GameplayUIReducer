/*
    COPYRIGHT NOTICE:
    © 2022 Thomas O'Sullivan - All rights reserved.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

    FILE INFORMATION:
    Name: Plugin.cs
    Project: GameplayUIReducer
    Author: Tom
    Created: 20th October 2022
*/

using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GameplayUIReducer
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static void HideElements()
        {
            foreach (var elementPath in ElementPaths)
            {
                bool isHidden = ConfigEntries[elementPath.Key]?.Value ?? false;
                if (isHidden) HideElement(elementPath.Key, elementPath.Value);
            }
        }

        public static bool IsRainbowVisible() => !ConfigEntries["Rainbow Borders"]?.Value ?? true;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}.");

            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            DeclareElements();

            Logger.LogInfo("Setting up configuration file...");
            SetupConfig();
        }

        private static void HideElement(string name, string objectPath)
        {
            if (string.IsNullOrWhiteSpace(objectPath)) return;
            GameObject gameObject = GameObject.Find(objectPath);
            if (gameObject == null)
            {
                Logger.LogError($"Unable to find {name} at '{objectPath}', it will not be hidden.");
            }
            else
            {
                Logger.LogDebug($"Hiding element {name}...");
                gameObject.SetActive(false);
            }
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
        }

        private void DeclareElements()
        {
            ElementPaths.Add("Breath Meter", "BreathCanvas");
            ElementPaths.Add("Champ Text", "ChampCanvas");
            ElementPaths.Add("Health Meter", "HealthMask");
            ElementPaths.Add("Rainbow Borders", null);
            ElementPaths.Add("Longest Combo", "GameplayCanvas/UIHolder/maxcombo");
            ElementPaths.Add("Note Lines", "GameplayCanvas/GameSpace/NoteLinesHolder");
            ElementPaths.Add("Lyrics", "GameplayCanvas/GameSpace/LyricsHolder");
            ElementPaths.Add("Note Explosions", "GameplayCanvas/GameSpace/NoteEndExplosions");
            ElementPaths.Add("Multiplier Popup", "GameplayCanvas/Popups/popup_mult");
            ElementPaths.Add("Accuracy Popup", "GameplayCanvas/Popups/popup_text");
            ElementPaths.Add("No Gap Popup", "GameplayCanvas/Popups/no_gap");
            ElementPaths.Add("Max Popup", "GameplayCanvas/Popups/MAX");
            ElementPaths.Add("Song Name", "GameplayCanvas/UIHolder/upper_right/Song Name Shadow");
            ElementPaths.Add("Score Counter", "GameplayCanvas/UIHolder/upper_right/ScoreShadow");
            ElementPaths.Add("Time Elapsed", "GameplayCanvas/UIHolder/time_elapsed");
        }

        private new static ManualLogSource Logger { get; set; }

        private static readonly Dictionary<string, ConfigEntry<bool>> ConfigEntries = new();
        private static readonly Dictionary<string, string> ElementPaths = new();
    }

    [HarmonyPatch(typeof(GameController), "Start")]
    internal class GameControllerStartPatch
    {
        static void Postfix()
        {
            Plugin.HideElements();
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
}
