using System.Collections;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using BetterMediaControls.util;

namespace BetterMediaControls.patches;

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.ChangeSong))]
[HarmonyPatch([typeof(bool)])]
public static class ShufflePatch
{
    private static readonly List<int> ShuffledOrder = new();
    private static int _shufflePosition;
    private static int _lastMixtapeIndex = -1;

    private static readonly MethodInfo ChangeSongClipMethod =
        AccessTools.Method(typeof(SoundManager), "ChangeSongClip");
    
    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool Prefix(SoundManager __instance, bool isNext)
    {
        if (!Plugin.Instance.IsShuffleEnabled)
            return true;
        
        var mixtapes = ReflectionUtils.GetField(__instance, "_mixtapes");
        var mixTapes = ReflectionUtils.GetField<IList>(mixtapes, "MixTapes");
        if (mixTapes == null || mixTapes.Count == 0)
            return true;

        int mixtapeIndex = (int)ReflectionUtils.GetField(__instance, "_mixtapeIndex");
        int currentIndex = (int)ReflectionUtils.GetField(__instance, "_songIndex");

        if (mixtapeIndex != _lastMixtapeIndex)
        {
            ShuffledOrder.Clear();
            _shufflePosition = 0;
            _lastMixtapeIndex = mixtapeIndex;
        }

        var songs = ReflectionUtils.GetField<IList>(mixTapes[mixtapeIndex], "Songs");
        if (songs == null || songs.Count == 0)
            return true;

        if (ShuffledOrder.Count != songs.Count)
        {
            ShuffledOrder.Clear();
            for (int i = 0; i < songs.Count; i++)
                ShuffledOrder.Add(i);

            Shuffle(ShuffledOrder);

            _shufflePosition = ShuffledOrder.IndexOf(currentIndex);
            if (_shufflePosition < 0)
                _shufflePosition = 0;
        }

        if (isNext)
        {
            _shufflePosition++;

            if (_shufflePosition >= ShuffledOrder.Count)
            {
                _shufflePosition = 0;
            }
        }
        else
        {
            _shufflePosition--;
            if (_shufflePosition < 0)
                _shufflePosition = ShuffledOrder.Count - 1;
        }

        int newIndex = ShuffledOrder[_shufflePosition];

        AccessTools.Field(__instance.GetType(), "_songIndex")
            .SetValue(__instance, newIndex);

        ChangeSongClipMethod.Invoke(__instance, null);

        MonoSingleton<UIManager>.I.UpdateSongInfo(
            (Song)songs[newIndex]
        );

        return false;
    }
    
    public static void ResetShuffle()
    {
        ShuffledOrder.Clear();
        _shufflePosition = 0;
    }
}