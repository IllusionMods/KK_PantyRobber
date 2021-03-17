using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ADV;
using BepInEx;
using Illusion.Game;
using Manager;
using UnityEngine;
using Info = ActionGame.Communication.Info;

namespace KK_PantyRobber
{
    internal static class Uty
    {
        internal static int panty = 3;

        internal static Utils.Sound.Setting seSet = new Utils.Sound.Setting(Manager.Sound.Type.GameSE3D);

        public static SaveData.Heroine GetCurrentVisibleGirl()
        {
            if (!Singleton<Game>.IsInstance()) return null;
            if (Singleton<Game>.Instance.actScene != null && Singleton<Game>.Instance.actScene.AdvScene != null)
            {
                var advScene = Singleton<Game>.Instance.actScene.AdvScene;
                if (advScene.Scenario?.currentHeroine != null) return advScene.Scenario.currentHeroine;
                TalkScene talkScene;
                if ((object)(talkScene = advScene.nowScene as TalkScene) != null && talkScene.targetHeroine != null)
                    return talkScene.targetHeroine;
            }

            return Object.FindObjectOfType<TalkScene>()?.targetHeroine;
        }

        private static ChaControl GetCurrentVisibleGirlChaControl()
        {
            var talkScene = Object.FindObjectOfType<TalkScene>();
            var heroine = talkScene != null ? talkScene.targetHeroine : null;
            if (heroine != null) return heroine.chaCtrl;
            var instance = Singleton<Game>.Instance;
            ADVScene aDVScene;
            if (instance == null)
            {
                aDVScene = null;
            }
            else
            {
                var actScene = instance.actScene;
                aDVScene = actScene != null ? actScene.AdvScene : null;
            }

            var aDVScene2 = aDVScene;
            if (aDVScene2 == null) return null;
            var scenario = aDVScene2.Scenario;
            if ((scenario != null ? scenario.currentHeroine : null) != null)
                return aDVScene2.Scenario.currentHeroine.chaCtrl;
            var instance2 = Singleton<Character>.Instance;
            if (instance2 != null && instance2.dictEntryChara.Count > 0) return instance2.dictEntryChara[0];
            try
            {
                return (typeof(ADVScene).GetField("m_TargetHeroine", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(aDVScene2.nowScene) as SaveData.Heroine)?.chaCtrl;
            }
            catch
            {
                return null;
            }
        }

        public static SaveData.Player GetPlayer()
        {
            return Singleton<Game>.Instance?.saveData?.player;
        }

        public static PantyRobberCharaController GetPlayerController()
        {
            return Singleton<Game>.Instance.actScene.Player?.chaCtrl?.gameObject
                .GetComponent<PantyRobberCharaController>();
        }

        public static List<Program.Transfer> GetTransfer(TalkScene talkScene, SaveData.Player player,
            SaveData.Heroine girl, string asset)
        {
            if (talkScene == null) return null;
            var list = new List<Program.Transfer>();
            Program.SetParam(player, girl, list);
            var files = Directory.GetFiles(Path.Combine(Paths.GameRootPath, "abdata\\adv\\scenario\\" + girl.ChaName), "??.unity3d");
            foreach (var path in files)
            {
                var assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset("adv/scenario/" + girl.ChaName + "/" + Path.GetFileName(path), asset, typeof(ScenarioData));
                if (assetBundleLoadAssetOperation == null) continue;
                var asset2 = assetBundleLoadAssetOperation.GetAsset<ScenarioData>();
                if (asset2 == null || asset2.list == null || asset2.list.Count == 0) continue;
                {
                    foreach (var item in asset2.list)
                        list.Add(Program.Transfer.Create(item.Multi, item.Command, item.Args));
                    return list;
                }
            }

            return null;
        }

        public static List<ScenarioData.Param> GetSenarioData(SaveData.Heroine girl, string asset)
        {
            var files = Directory.GetFiles(Path.Combine(Paths.GameRootPath, "abdata\\adv\\scenario\\" + girl.ChaName),
                "??.unity3d");
            foreach (var path in files)
            {
                var assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(
                    "adv/scenario/" + girl.ChaName + "/" + Path.GetFileName(path), asset, typeof(ScenarioData));
                if (assetBundleLoadAssetOperation != null)
                {
                    var asset2 = assetBundleLoadAssetOperation.GetAsset<ScenarioData>();
                    if (!(asset2 == null) && asset2.list != null && asset2.list.Count != 0) return asset2.list;
                }
            }

            return null;
        }

        public static Info GetInfo(TalkScene talkScene)
        {
            return (Info)typeof(TalkScene).InvokeMember("info",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, talkScene, null);
        }

        public static void TouchMuneL(TalkScene talkScene)
        {
            typeof(TalkScene).InvokeMember("TouchFunc",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, talkScene,
                new object[]
                {
                    "MuneL",
                    new Vector3(0f, 0f, 0f)
                });
        }

        public static void StartADV(TalkScene talkScene, List<Program.Transfer> _list)
        {
            typeof(TalkScene).InvokeMember("StartADV",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, talkScene,
                new object[]
                {
                    _list
                });
        }

        public static IEnumerator HeroineEventWait(TalkScene talkScene)
        {
            return (IEnumerator)typeof(TalkScene).InvokeMember("HeroineEventWait",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, talkScene, null);
        }

        public static IEnumerator TalkEnd(TalkScene talkScene)
        {
            return (IEnumerator)typeof(TalkScene).InvokeMember("TalkEnd",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, talkScene, null);
        }

        public static Texture2D GetTexture(ChaControl chaControl, ChaListDefine.CategoryNo type, int id,
            ChaListDefine.KeyType assetBundleKey, ChaListDefine.KeyType assetKey, string addStr = "")
        {
            return (Texture2D)typeof(ChaControl).InvokeMember("GetTexture",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, chaControl,
                new object[]
                {
                    type,
                    id,
                    assetBundleKey,
                    assetKey,
                    addStr
                });
        }

        public static void ApplyNoPanty(SaveData.Heroine girl, int coodType, bool restore = true)
        {
            var coordinateType = girl.chaCtrl.fileStatus.coordinateType;
            PantyRobber.Log($"ApplyNoPanty={girl.Name} coodType={(ChaFileDefine.CoordinateType)coodType}");
            if (coodType != coordinateType)
                girl.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coodType, false);
            girl.charFile.coordinate[coodType].clothes.parts[panty].id = 0;
            var data = girl.charFile.coordinate[coodType].SaveBytes();
            girl.chaCtrl.nowCoordinate.LoadBytes(data, ChaFileDefine.ChaFileCoordinateVersion);
            girl.chaCtrl.chaFile.coordinate[coodType].LoadBytes(data, ChaFileDefine.ChaFileCoordinateVersion);
            girl.chaCtrl.Reload(false, true, true, true);
            girl.charFile.coordinate[coodType].clothes.parts[panty].id = 0;
            girl.chaCtrl.chaFile.coordinate[coodType].clothes.parts[panty].id = 0;
            girl.chaCtrl.nowCoordinate.clothes.parts[panty].id = 0;
            if (restore)
                girl.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coordinateType, false);
        }

        public static void ApplyNoPanty(SaveData.Heroine girl)
        {
            PantyRobber.Log("ApplyNoPanty=" + girl.Name);
            var coordinateType = girl.chaCtrl.fileStatus.coordinateType;
            girl.charFile.coordinate[coordinateType].clothes.parts[panty].id = 0;
            var data = girl.charFile.coordinate[coordinateType].SaveBytes();
            girl.chaCtrl.nowCoordinate.LoadBytes(data, ChaFileDefine.ChaFileCoordinateVersion);
            girl.chaCtrl.chaFile.coordinate[coordinateType].LoadBytes(data, ChaFileDefine.ChaFileCoordinateVersion);
            girl.chaCtrl.Reload(false, true, true, true);
            girl.charFile.coordinate[coordinateType].clothes.parts[panty].id = 0;
            girl.chaCtrl.chaFile.coordinate[coordinateType].clothes.parts[panty].id = 0;
            girl.chaCtrl.nowCoordinate.clothes.parts[panty].id = 0;
        }

        public static void playSE(ChaControl cha, ChaReference.RefObjKey reference, string asset, string file)
        {
            seSet.assetBundleName = asset;
            seSet.assetName = file;
            var transform = Utils.Sound.Play(seSet);
            if (cha && transform)
            {
                var referenceInfo = cha.GetReferenceInfo(reference);
                if (referenceInfo) transform.SetParent(referenceInfo.transform, false);
            }
        }

        public static bool Probability(float fPercent)
        {
            var num = Random.value * 100f;
            if (fPercent == 0f) return false;
            if (fPercent == 100f && num == fPercent) return true;
            if (num < fPercent) return true;
            return false;
        }
    }
}