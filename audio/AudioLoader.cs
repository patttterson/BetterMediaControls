using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterMediaControls.audio
{
    public static class AudioLoader
    {
        private static readonly string[] SupportedExtensions = { ".wav", ".ogg", ".mp3" };

        public static bool IsSupportedFile(string path)
        {
            var ext = Path.GetExtension(path);
            foreach (var supported in SupportedExtensions)
            {
                if (string.Equals(ext, supported, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static IEnumerator LoadAudio(string path, Action<AudioClip> onLoaded)
        {
            var uri = "file:///" + path.Replace("\\", "/");
            var audioType = GetAudioType(path);

            using (var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
            {
                ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Plugin.Log.LogError($"Failed to load audio {path}: {request.error}");
                    onLoaded(null);
                    yield break;
                }

                var clip = DownloadHandlerAudioClip.GetContent(request);
                clip.name = Path.GetFileNameWithoutExtension(path);
                onLoaded(clip);
            }
        }

        private static AudioType GetAudioType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            switch (ext)
            {
                case ".ogg": return AudioType.OGGVORBIS;
                case ".mp3": return AudioType.MPEG;
                case ".wav": return AudioType.WAV;
                default: return AudioType.UNKNOWN;
            }
        }
    }
}
