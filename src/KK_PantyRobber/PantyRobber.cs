/*
 * This plugin was originally made by picolet21. This codebase is based on v0.2 of his KK_PantyRobber plugin.
 * He wrote most of it and figured out how to create custom talk scenes. Big respect to him!
 *
 * I started adding to this plugin because the development seemed to have stopped (no updates in half a year).
 * My additions include everything in the git history. First commit is the original v0.2 code.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Illusion.Game;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Utilities;
using UnityEngine;

namespace KK_PantyRobber
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    public class PantyRobber : BaseUnityPlugin
    {
        public enum DefaultOtherSteal_EN
        {
            DoNotSteal,
            SamePantiesOnly,
            StealEverything
        }

        public enum DefaultOtherSteal_JP
        {
            他は強奪しない,
            同じショーツの場合,
            全て強奪
        }

        public const string GUID = "picolet21.koikatsu.PantyRobber";
        public const string PluginName = "PantyRobber";
        public const string Version = "0.2";

        internal static PantyRobber Instance;
        internal static new ManualLogSource Logger { get; private set; }

        public static ConfigEntry<bool> EnablePantyRobber { get; private set; }
        public static ConfigEntry<KeyboardShortcut> StealKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ResetKey { get; private set; }
        public static ConfigEntry<bool> HalfOff { get; private set; }
        public static ConfigEntry<bool> GirlReaction { get; private set; }
        public static ConfigEntry<bool> AlwaysSuccessful { get; private set; }
        public static ConfigEntry<bool> WithoutBottoms { get; private set; }

        public static DefaultOtherSteal_EN DefaultOtherSteal => DefaultOtherSteal_EN.DoNotSteal; //todo handle stealing from other coordinates

        public static int Language
        {
            get
            {
                if (Application.systemLanguage != SystemLanguage.Japanese) return 1;
                return 0;
            }
        }

        private void Awake()
        {
            if (StudioAPI.InsideStudio) throw new NotImplementedException("Shouldn't load in studio");

            Instance = this;
            Logger = base.Logger;

            if (Language == 0)
            {
                EnablePantyRobber = Config.Bind("", "ショーツ強奪を有効", true, new ConfigDescription("会話中に、ショーツ強奪を可能にします。", null, new ConfigurationManagerAttributes { Order = 21 }));
                StealKey = Config.Bind("", "ショーツ強奪キー", new KeyboardShortcut(KeyCode.Return), new ConfigDescription("会話中に、ここで設定したキーを押すと、ショーツ強奪を試みます。", null, new ConfigurationManagerAttributes { Order = 20 }));
                GirlReaction = Config.Bind("Options", "女の子のリアクション", true, new ConfigDescription("ショーツ強奪に成功したとき、女の子がリアクションをします。 今のところ、そのリアクションは胸を触ったときと同じものです。", null, new ConfigurationManagerAttributes { Order = 14 }));
                WithoutBottoms = Config.Bind("Options", "ボトムスなしは強奪不可", false, new ConfigDescription("ボトムスを履いていない場合は強奪できなくします。", null, new ConfigurationManagerAttributes { Order = 13 }));
                HalfOff = Config.Bind("Options", "ボトムスを半脱ぎにする", false, new ConfigDescription("ショーツ強奪に成功したら、パンストとボトムスを半脱ぎにします。", null, new ConfigurationManagerAttributes { Order = 12 }));
                AlwaysSuccessful = Config.Bind("Options", "常に強奪成功", false, new ConfigDescription("有効にすると、ショーツ強奪に常に成功します。 無効の場合は確率判断となり、処女の場合は失敗しやすく、淫乱なほど成功し易くなります。 \nなお、失敗した場合は女の子が怒ります。", null, new ConfigurationManagerAttributes { Order = 11 }));
                ResetKey = Config.Bind("Options", "シナリオリセットキー", new KeyboardShortcut(KeyCode.Return, KeyCode.LeftControl), new ConfigDescription("シナリオをリセットして最初から始めます。スティールレベルもリセットされます。", null, new ConfigurationManagerAttributes { Order = 10 }));
            }
            else
            {
                EnablePantyRobber = Config.Bind("", "Enable", true, new ConfigDescription("Allows you to snatch panties during a conversation.", null, new ConfigurationManagerAttributes { Order = 21 }));
                StealKey = Config.Bind("", "Panty robbery key", new KeyboardShortcut(KeyCode.Return), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 20 }));
                GirlReaction = Config.Bind("", "Girl reacts", true, new ConfigDescription("When you succeed in stealing panties, the girl reacts.", null, new ConfigurationManagerAttributes { Order = 14 }));
                WithoutBottoms = Config.Bind("Options", "No robbery without bottoms", false, new ConfigDescription("If you don't wear bottoms, you won't be able to rob.", null, new ConfigurationManagerAttributes { Order = 13 }));
                HalfOff = Config.Bind("", "Take off the bottoms halfway", false, new ConfigDescription("After successfully robbing the panties, take off the girl's pantyhose and bottoms.", null, new ConfigurationManagerAttributes { Order = 12 }));
                AlwaysSuccessful = Config.Bind("", "Always successful in robbing", false, new ConfigDescription("When enabled, it will always succeed in robbing panties. Otherwise, it will be a probability judgment. Virgin women are more likely to fail, and the more nasty they are to succeed.\nIf you fail, the girl gets angry.", null, new ConfigurationManagerAttributes { Order = 11 }));
                ResetKey = Config.Bind("Options", "Senario reset key", new KeyboardShortcut(KeyCode.Return, KeyCode.LeftControl), new ConfigDescription("Reset the scenario and start from the beginning. The steal level will also be reset.", null, new ConfigurationManagerAttributes { Order = 10 }));
            }

            CharacterApi.RegisterExtraBehaviour<PantyRobberCharaController>(GUID);

            GameAPI.EndH += (sender, args) =>
            {
                foreach (var item in FindObjectOfType<HSceneProc>().flags.lstHeroine)
                    StartCoroutine(RefreshOnSceneChangeCo(item));
            };

            Hooks.InstallHooks();
        }

        private static readonly List<SaveData.Heroine> _noPantyChara = new List<SaveData.Heroine>();

        internal void OnSceneUnload(SaveData.Heroine heroine, ChaControl controller)
        {
            if (!StudioAPI.InsideStudio && !MakerAPI.InsideMaker && EnablePantyRobber.Value)
            {
                Log("OnSceneUnload");
                StartCoroutine(RefreshOnSceneChangeCo(heroine));
            }
        }

        private static IEnumerator RefreshOnSceneChangeCo(SaveData.Heroine girl)
        {
            if (_noPantyChara.Contains(girl))
            {
                Log("RefreshOnSceneChangeCo");
                var previousControl = girl.chaCtrl;
                yield return new WaitUntil(() => girl.chaCtrl != previousControl && girl.chaCtrl != null);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                _noPantyChara.Remove(girl);
                Uty.ApplyNoPanty(girl);
                if (girl.chaCtrl.fileParam.sex == 1) girl.chaCtrl.fileStatus.visibleSonAlways = false; //bug?
            }
        }

        private void LateUpdate()
        {
            if (GameAPI.InsideHScene || MakerAPI.InsideMaker) return;

            if (ResetKey.Value.IsDown())
            {
                Log("ResetKey");
                var playerController = Uty.GetPlayerController();
                playerController.Data.StealLevel = -1;
                playerController.SaveData();
                Utils.Sound.Play(SystemSE.ok_s);
            }
            else if (StealKey.Value.IsDown() && EnablePantyRobber.Value)
            {
                Log("StealKey");
                var talkScene = FindObjectOfType<TalkScene>();
                if (talkScene != null)
                {
                    var currentVisibleGirl = Uty.GetCurrentVisibleGirl();
                    if (TalkLevelSpawner.StealPanty(talkScene, currentVisibleGirl))
                        _noPantyChara.Add(currentVisibleGirl);
                }
                else
                {
                    StartCoroutine(TalkLevelSpawner.SpawnLevel());
                }
            }
        }

        public static void Log(string msg, bool isErr = false)
        {
            if (isErr) Logger.LogError(msg);
#if DEBUG
            Logger.LogDebug(msg);
#endif
        }
    }
}