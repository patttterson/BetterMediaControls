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
    private const string PluginName = "BetterMediaControls";
    private const string PluginVersion = "1.1.1";
    private const string PluginAuthor = "CutiePatooties"; // as seen on thunderstore

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
            Path.Combine(Paths.ConfigPath, "BetterMediaControls-Music"),
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
            $"{PluginAuthor}-{PluginName}",
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

        var musicDir = GetMusicDirectory();
        if (!Directory.Exists(musicDir))
        {
            try
            {
                Directory.CreateDirectory(musicDir);
                Log.LogInfo($"Created music directory at {musicDir}");
            }
            catch (System.Exception e)
            {
                Log.LogError($"Failed to create music directory at {musicDir}: {e}");
            }
        }

        var playlistPath = Path.Combine(musicDir, "playlist.json");
        if (!File.Exists(playlistPath))
        {
            try
            {
                File.WriteAllText(playlistPath, "{\"playlist\":[]}");
                Log.LogInfo($"Created empty playlist.json at {playlistPath}");
            }
            catch (System.Exception e)
            {
                Log.LogError($"Failed to create playlist.json at {playlistPath}: {e}");
            }
        }

        Logger.LogInfo($"Plugin {PluginGUID} is loaded!");
        _harmony.PatchAll();
    }

    public void ToggleShuffle()
    {
        _configShuffleEnabled.Value = !_configShuffleEnabled.Value;
        if (_configShuffleEnabled.Value)
        {
            patches.ShufflePatch.ResetShuffle();
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
            Paths.ConfigPath,
            "BetterMediaControls-Music"
        );
    }
}