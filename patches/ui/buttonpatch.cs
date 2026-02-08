using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

using BetterMediaControls.util;

namespace BetterMediaControls.patches.ui;

[HarmonyPatch(typeof(UIManager), "Start")]
public static class ShuffleUIPatch
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(UIManager __instance)
    {
        var log = Plugin.Log;

        var loopImage = ReflectionUtils.GetField<Image>(__instance, "_loopImage");
        if (loopImage == null)
        {
            log.LogError("Shuffle UI: _loopImage not found");
            return;
        }

        // ReSharper disable twice InconsistentNaming
        var loopGO = loopImage.gameObject;
        var shuffleGO = Object.Instantiate(loopGO, loopGO.transform.parent);
        shuffleGO.name = "Button_Shuffle";

        shuffleGO.transform.localPosition += new Vector3(60f, 0f, 0f);

        var shuffleImage = shuffleGO.GetComponent<Image>();
        var shuffleButton = shuffleGO.GetComponent<Button>();

        if (shuffleButton == null || shuffleImage == null)
        {
            log.LogError("Shuffle UI: missing Image or Button component");
            return;
        }

        shuffleButton.onClick = new Button.ButtonClickedEvent();
        shuffleButton.onClick.AddListener(() =>
        {
            Plugin.Instance.ToggleShuffle();
            UpdateShuffleUI(__instance, shuffleImage);
            SFXManager.I.PlayUIClick();
        });

        UpdateShuffleUI(__instance, shuffleImage);

        log.LogInfo("Shuffle button cloned from loop button");
    }

    private static void UpdateShuffleUI(UIManager uiManager, Image shuffleImage)
    {
        bool enabled = Plugin.Instance.IsShuffleEnabled;

        var checkedSprite =
            Plugin.Instance.ShuffleOnSprite
            ?? ReflectionUtils.GetField<Sprite>(uiManager, "_loopCheckedSprite");

        var uncheckedSprite =
            Plugin.Instance.ShuffleOffSprite
            ?? ReflectionUtils.GetField<Sprite>(uiManager, "_loopUncheckedSprite");

        shuffleImage.sprite = enabled ? checkedSprite : uncheckedSprite;
    }
}
