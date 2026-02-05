using System.IO;
using HarmonyLib;

using System.Collections;
using BetterMediaControls.audio;
using BetterMediaControls.util;
using Newtonsoft.Json;

namespace BetterMediaControls.patches;

[HarmonyPatch(typeof(SoundManager))]
public static class MediaPatch
{
    [HarmonyPatch("Start")]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(SoundManager __instance)
    {
        __instance.StartCoroutine(InjectSong(__instance));
    }

    private static IEnumerator InjectSong(SoundManager soundManager)
    {
        var log = Plugin.Log;
        if (!Plugin.Instance)
        {
            log.LogError("Plugin instance is null");
            yield break;
        }
        var musicDir = Plugin.Instance.GetMusicDirectory();
        
        var playlistPath = Path.Combine(musicDir, "playlist.json");
        PlaylistFile playlistFile = null;

        bool hasPlaylistJson = File.Exists(playlistPath);
        
        if (!hasPlaylistJson)
        {
            log.LogWarning($"No playlist.json found at {playlistPath}, falling back to directory scan");
        }
        else
        {
            try
            {
                var json = File.ReadAllText(playlistPath);
                playlistFile = JsonConvert.DeserializeObject<PlaylistFile>(json);

                if (playlistFile?.Playlist == null)
                {
                    log.LogError("playlist.json parsed, but playlist is null");
                    yield break;
                }

                log.LogDebug($"Parsed playlist.json with {playlistFile.Playlist.Count} entries");
            }
            catch (System.Exception e)
            {
                log.LogError($"Failed to parse playlist.json: {e}");
                yield break;
            }
        }

        var mixtapesObj = ReflectionUtils.GetField(soundManager, "_mixtapes");
        if (mixtapesObj == null)
        {
            log.LogError("_mixtapes is null");
            yield break;
        }

        var mixTapes = ReflectionUtils.GetField<IList>(mixtapesObj, "MixTapes");
        if (mixTapes == null || mixTapes.Count == 0)
        {
            log.LogError("MixTapes is null or empty");
            yield break;
        }

        var firstMixTape = mixTapes[0];

        var songs = ReflectionUtils.GetField<IList>(firstMixTape, "Songs");
        if (songs == null)
        {
            log.LogError("Songs is null");
            yield break;
        }

        if (!Plugin.Instance.IsVanillaMusicEnabled)
        {
            songs.Clear();
        }

        if (hasPlaylistJson)
        {
            // inverted so when we insert at 0, the order is preserved  
            for (int i = playlistFile.Playlist.Count - 1; i >= 0; i--)
            {
                var entry = playlistFile.Playlist[i];
                var audioPath = Path.Combine(musicDir, entry.File);

                if (!File.Exists(audioPath))
                {
                    log.LogError($"Missing audio file: {audioPath}");
                    continue;
                }

                log.LogDebug($"Loading song: {audioPath}");

                UnityEngine.AudioClip clip = null;
                yield return AudioLoader.LoadAudio(audioPath, c => clip = c);

                if (!clip)
                {
                    log.LogError($"Failed to load audio: {audioPath}");
                    continue;
                }

                var song = new Song
                {
                    Title = string.IsNullOrEmpty(entry.Title)
                        ? Path.GetFileNameWithoutExtension(entry.File)
                        : entry.Title,

                    Artist = string.IsNullOrEmpty(entry.Artist)
                        ? "Unknown"
                        : entry.Artist,

                    Music = clip,
                    Volume = entry.Volume > 0f ? entry.Volume : 1.0f
                };

                songs.Insert(0, song);
                log.LogDebug($"Added song: {song.Title} by {song.Artist}");
            }
        }
        else
        {
            var files = Directory.GetFiles(musicDir);

            for (int i = files.Length - 1; i >= 0; i--)
            {
                var audioPath = files[i];
                if (!AudioLoader.IsSupportedFile(audioPath))
                    continue;

                UnityEngine.AudioClip clip = null;
                yield return AudioLoader.LoadAudio(audioPath, c => clip = c);

                if (!clip)
                {
                    log.LogError($"Failed to load audio: {audioPath}");
                    continue;
                }

                var song = new Song
                {
                    Title = Path.GetFileNameWithoutExtension(audioPath),
                    Artist = "Unknown",
                    Music = clip,
                    Volume = 1.0f
                };

                songs.Insert(0, song);
                log.LogDebug($"Added fallback song: {song.Title}");
            }
        }

        log.LogInfo($"Final song count: {songs.Count}");
    }
}