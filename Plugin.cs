using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterMediaControls.util;
using HarmonyLib;
using UnityEngine;

namespace BetterMediaControls;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInProcess("OnTogether.exe")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }

    // ReSharper disable once InconsistentNaming
    private const string PluginGUID = "com.patty.bettermediacontrols";
    private const string PluginName = "Better Media Controls";
    private const string PluginVersion = "0.1.0";

    private readonly Harmony _harmony = new Harmony(PluginGUID);

    private ConfigEntry<string> _configMusicDir;
    private ConfigEntry<bool> _configVanillaMusicEnabled;
    private ConfigEntry<bool> _configShuffleEnabled;

    public bool IsVanillaMusicEnabled => _configVanillaMusicEnabled.Value;
    public bool IsShuffleEnabled => _configShuffleEnabled.Value;

    public static ManualLogSource Log { get; private set; }
    
    public Sprite ShuffleOnSprite { get; private set; }
    public Sprite ShuffleOffSprite { get; private set; }

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        _configMusicDir = Config.Bind(
            "General",
            "MusicDirectory",
            "music",
            "Supports .wav, .ogg, and .mp3 files. Directory inside the plugin folder where custom music is stored."
        );

        _configVanillaMusicEnabled = Config.Bind(
            "General",
            "EnableVanillaMusic",
            true,
            "If true, the game's original music will still show up alongside custom tracks."
        );

        _configShuffleEnabled = Config.Bind(
            "Playback",
            "EnableShuffle",
            false,
            "self explanatory. uses spotify shuffle (Fisher–Yates) algorithm."
        );
        
        var iconsDir = Path.Combine(
            Paths.PluginPath,
            "BetterMediaControls",
            "icons"
        );

        ShuffleOnSprite = SpriteLoader.LoadSprite(
            Path.Combine(iconsDir, "Shuffle.png")
        );

        ShuffleOffSprite = SpriteLoader.LoadSprite(
            Path.Combine(iconsDir, "ShuffleUnchecked.png")
        );

        if (ShuffleOnSprite == null || ShuffleOffSprite == null)
        {
            Log.LogWarning("Shuffle icons missing or failed to load");
        }

        Logger.LogInfo($"Plugin {PluginGUID} is loaded!");
        _harmony.PatchAll();
    }

    public void ToggleShuffle()
    {
        _configShuffleEnabled.Value = !_configShuffleEnabled.Value;
        if (_configShuffleEnabled.Value)
        {
            BetterMediaControls.patches.ShufflePatch.ResetShuffle();
        }
        Log.LogInfo($"Shuffle {(IsShuffleEnabled ? "enabled" : "disabled")}");
    }

    public string GetMusicDirectory()
    {
        var value = _configMusicDir.Value;
        value = System.Environment.ExpandEnvironmentVariables(value);

        if (Path.IsPathRooted(value))
            return value;

        return Path.Combine(
            Paths.PluginPath,
            "BetterMediaControls",
            value
        );
    }
}
