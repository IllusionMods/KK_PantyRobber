using System;
using System.Collections;
using System.Collections.Generic;
using ActionGame.Chara;
using ADV;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Extension;
using Illusion.Game;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KK_PantyRobber
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency("marco.kkapi", "1.4")]
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

        public const string PluginNameInternal = "KK_PantyRobber";

        public const string Version = "0.2";

        internal static PantyRobber Instance;

        internal static bool _isDuringHScene;

        internal static ManualLogSource Logger { get; private set; }

        public static ConfigEntry<bool> EnablePantyRobber { get; private set; }

        public static ConfigEntry<KeyboardShortcut> StealKey { get; private set; }

        public static ConfigEntry<KeyboardShortcut> ResetKey { get; private set; }

        public static ConfigEntry<bool> HalfOff { get; private set; }

        public static ConfigEntry<bool> GirlReaction { get; private set; }

        public static ConfigEntry<bool> AlwaysSuccessful { get; private set; }

        public static ConfigEntry<bool> WithoutBottoms { get; private set; }

        public static DefaultOtherSteal_EN DefaultOtherSteal => DefaultOtherSteal_EN.DoNotSteal;

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
            if (!StudioAPI.InsideStudio)
            {
                Instance = this;
                if (Language == 0)
                {
                    EnablePantyRobber = Config.Bind("", "ショーツ強奪を有効", true, new ConfigDescription("会話中に、ショーツ強奪を可能にします。",
                        null, new ConfigurationManagerAttributes
                        {
                            Order = 21
                        }));
                    StealKey = Config.Bind("", "ショーツ強奪キー", new KeyboardShortcut(KeyCode.Return), new ConfigDescription(
                        "会話中に、ここで設定したキーを押すと、ショーツ強奪を試みます。", null, new ConfigurationManagerAttributes
                        {
                            Order = 20
                        }));
                    GirlReaction = Config.Bind("Options", "女の子のリアクション", true, new ConfigDescription(
                        "ショーツ強奪に成功したとき、女の子がリアクションをします。 今のところ、そのリアクションは胸を触ったときと同じものです。", null,
                        new ConfigurationManagerAttributes
                        {
                            Order = 14
                        }));
                    WithoutBottoms = Config.Bind("Options", "ボトムスなしは強奪不可", false, new ConfigDescription(
                        "ボトムスを履いていない場合は強奪できなくします。", null, new ConfigurationManagerAttributes
                        {
                            Order = 13
                        }));
                    HalfOff = Config.Bind("Options", "ボトムスを半脱ぎにする", false, new ConfigDescription(
                        "ショーツ強奪に成功したら、パンストとボトムスを半脱ぎにします。", null, new ConfigurationManagerAttributes
                        {
                            Order = 12
                        }));
                    AlwaysSuccessful = Config.Bind("Options", "常に強奪成功", false, new ConfigDescription(
                        "有効にすると、ショーツ強奪に常に成功します。 無効の場合は確率判断となり、処女の場合は失敗しやすく、淫乱なほど成功し易くなります。 \nなお、失敗した場合は女の子が怒ります。", null,
                        new ConfigurationManagerAttributes
                        {
                            Order = 11
                        }));
                    ResetKey = Config.Bind("Options", "シナリオリセットキー",
                        new KeyboardShortcut(KeyCode.Return, KeyCode.LeftControl), new ConfigDescription(
                            "シナリオをリセットして最初から始めます。スティールレベルもリセットされます。", null, new ConfigurationManagerAttributes
                            {
                                Order = 10
                            }));
                }
                else
                {
                    EnablePantyRobber = Config.Bind("", "Enable", true, new ConfigDescription(
                        "Allows you to snatch panties during a conversation.", null, new ConfigurationManagerAttributes
                        {
                            Order = 21
                        }));
                    StealKey = Config.Bind("", "Panty robbery key", new KeyboardShortcut(KeyCode.KeypadEnter),
                        new ConfigDescription("", null, new ConfigurationManagerAttributes
                        {
                            Order = 20
                        }));
                    GirlReaction = Config.Bind("", "Girl reacts", true, new ConfigDescription(
                        "When you succeed in stealing panties, the girl reacts.", null,
                        new ConfigurationManagerAttributes
                        {
                            Order = 14
                        }));
                    WithoutBottoms = Config.Bind("Options", "No robbery without bottoms", false, new ConfigDescription(
                        "If you don't wear bottoms, you won't be able to rob.", null, new ConfigurationManagerAttributes
                        {
                            Order = 13
                        }));
                    HalfOff = Config.Bind("", "Take off the bottoms halfway", false, new ConfigDescription(
                        "After successfully robbing the panties, take off the girl's pantyhose and bottoms.", null,
                        new ConfigurationManagerAttributes
                        {
                            Order = 12
                        }));
                    AlwaysSuccessful = Config.Bind("", "Always successful in robbing", false, new ConfigDescription(
                        "When enabled, it will always succeed in robbing panties. Otherwise, it will be a probability judgment. Virgin women are more likely to fail, and the more nasty they are to succeed.\nIf you fail, the girl gets angry.",
                        null, new ConfigurationManagerAttributes
                        {
                            Order = 11
                        }));
                    ResetKey = Config.Bind("Options", "Senario reset key",
                        new KeyboardShortcut(KeyCode.Return, KeyCode.LeftControl), new ConfigDescription(
                            "Reset the scenario and start from the beginning. The steal level will also be reset.",
                            null, new ConfigurationManagerAttributes
                            {
                                Order = 10
                            }));
                }

                Logger = base.Logger;
                Log("PantyRobber Start.");
                CharacterApi.RegisterExtraBehaviour<PantyRobberCharaController>(GUID);
                GameAPI.RegisterExtraBehaviour<PantyRobberGameController>(GUID);
                Hooks.InstallHooks();
            }
        }

        private IEnumerator TalkLevel()
        {
            var talkScene = FindObjectOfType<TalkScene>();
            if (talkScene == null) yield break;
            var player = Uty.GetPlayer();
            if (player != null && Uty.GetCurrentVisibleGirl() != null)
            {
                var list = new List<Program.Transfer>();
                Program.SetParam(player, list);
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "ちかりん"));
                list.Add(Program.Transfer.Text("[P名]", "「スティール！」"));
                list.Add(Program.Transfer.Text("[P名]", "（わっ、本当にできた……）"));
                list.Add(Program.Transfer.Create(true, Command.SE2DPlay, "sound/data/pcm/c08/adm/00.unity3d",
                    "adm_00_08_031_04", "0", "0", "TRUE", "TRUE", "-1", "FALSE", "FALSE"));
                list.Add(Program.Transfer.Text("ちかりん", "（その調子です……もっともっとスティールするのです……）"));
                list.Add(Program.Transfer.Close());
                Uty.StartADV(talkScene, list);
                yield return null;
                yield return Program.Wait("Talk");
                Observable.FromCoroutine(() => Uty.TalkEnd(talkScene)).Subscribe().AddTo(this);
            }
        }

        private IEnumerator SpawnLevel()
        {
            Log("SpawnLevel");
            var actScene = Singleton<Game>.Instance.actScene;
            var player = Uty.GetPlayer();
            if (player == null) yield break;
            var playerStatus = Uty.GetPlayerController();
            Log($"StealLevel={playerStatus.Data.StealLevel}");
            bool dialogOnly;
            List<Program.Transfer> list;
            switch (playerStatus.Data.StealLevel)
            {
                case -1:
                    dialogOnly = false;
                    list = SpawnLevel0(player);
                    break;
                case 0:
                    dialogOnly = true;
                    list = new List<Program.Transfer>();
                    Program.SetParam(player, list);
                    list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "ちかりん"));
                    list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d", "adm_02_08_001"));
                    list.Add(Program.Transfer.Text("ちかりん", "（ふふふ……スティールするのですよ……）"));
                    list.Add(Program.Transfer.Close());
                    break;
                default:
                    {
                        dialogOnly = true;
                        list = new List<Program.Transfer>();
                        Program.SetParam(player, list);
                        var array = new string[6]
                        {
                        "adm_00_08_003",
                        "adm_00_08_005",
                        "adm_00_08_007",
                        "adm_00_08_016",
                        "adm_00_08_021",
                        "adm_01_08_004"
                        };
                        var array2 = new string[4]
                        {
                        "シコってもいいのよ",
                        "嗅いでもいいのよ",
                        "頭に被ってもいいのよ",
                        "履いてみてもいいのよ"
                        };
                        list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "ちかりん"));
                        list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d",
                            array[Random.Range(0, array.Length)]));
                        list.Add(Program.Transfer.Text("ちかりん",
                            "（その調子です……もっとスティールするのです……" + array2[Random.Range(0, array2.Length)] + "……）"));
                        list.Add(Program.Transfer.Close());
                        break;
                    }
            }

            _ = actScene.Cycle.nowType;
            var prevBGM = string.Empty;
            var prevVolume = 1f;
            if (!dialogOnly)
            {
                actScene.SetProperty("_isInChargeBGM", true);
                actScene.Player.isActionNow = true;
                actScene.SetProperty("isEventNow", true);
                actScene.Player.isLesMotionPlay = false;
                actScene.SetProperty("shortcutKey", false);
                yield return StartCoroutine(Utils.Sound.GetBGMandVolume(delegate (string bgm, float volume)
                {
                    prevBGM = bgm;
                    prevVolume = volume;
                }));
                yield return null;
                yield return new WaitUntil(() => Singleton<Scene>.Instance.AddSceneName.IsNullOrEmpty());
                actScene.Player.SetActive(false);
                actScene.npcList.ForEach(delegate (NPC p) { p.SetActive(false); });
                actScene.npcList.ForEach(delegate (NPC p) { p.Pause(true); });
            }

            var position = actScene.Player.position;
            var rotation = actScene.Player.rotation;
            OpenData.CameraData camera = null;
            if (dialogOnly)
                camera = new OpenData.CameraData
                {
                    position = actScene.cameraTransform.position,
                    rotation = actScene.cameraTransform.rotation
                };
            var isOpenADV = false;
            yield return StartCoroutine(Program.Open(new Data
            {
                fadeInTime = 0f,
                position = position,
                rotation = rotation,
                camera = camera,
                heroineList = new List<SaveData.Heroine>(),
                scene = actScene,
                transferList = list
            }, new Program.OpenDataProc
            {
                onLoad = delegate { isOpenADV = true; }
            }));
            yield return new WaitUntil(() => isOpenADV);
            yield return Program.Wait(string.Empty);
            if (!dialogOnly)
            {
                actScene.npcList.ForEach(delegate (NPC p) { p.SetActive(p.mapNo == actScene.Map.no); });
                actScene.npcList.ForEach(delegate (NPC p)
                {
                    p.Pause(false);
                    p.isPopOK = true;
                    p.AI.FirstAction();
                    p.ReStart();
                });
                yield return new WaitUntil(() => actScene.MiniMapAndCameraActive);
                if (Utils.Scene.IsFadeOutOK)
                    yield return StartCoroutine(Singleton<Scene>.Instance.Fade(SimpleFade.Fade.Out));
                yield return StartCoroutine(Utils.Sound.GetFadePlayerWhileNull(prevBGM, prevVolume));
                actScene.SetProperty("shortcutKey", true);
                actScene.SetProperty("_isInChargeBGM", false);
                actScene.SetProperty("isEventNow", false);
                actScene.Player.isActionNow = false;
                actScene.Player.move.isStop = false;
                actScene.Player.isPopOK = true;
                actScene.Player.SetActive(true);
            }

            if (playerStatus.Data.StealLevel == -1)
            {
                playerStatus.Data.StealLevel = 0;
                playerStatus.SaveData();
            }
        }

        private List<Program.Transfer> SpawnLevel0(SaveData.Player player)
        {
            var list = new List<Program.Transfer>();
            Program.SetParam(player, list);
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "？？？"));
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "ちかりん"));
            list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0", "white", "back", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.NullSet, "adv_ev_00_camera", "Camera"));
            list.Add(Program.Transfer.Create(false, Command.NullSet, "adv_ev_00_chara", "Chara"));
            list.Add(Program.Transfer.Create(false, Command.BGMPlay, "Encounter", "", "0", "", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.CameraLock, "TRUE"));
            list.Add(Program.Transfer.Create(true, Command.CharaMobCreate, "0", "custom/presets_f_00.unity3d",
                "ill_Default_Female"));
            list.Add(Program.Transfer.Create(false, Command.CharaActive, "0", "FALSE", "center"));
            list.Add(Program.Transfer.Create(false, Command.Fade, "out", ".5", "white", "back", "TRUE"));
            list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d", "adm_03_08_004"));
            list.Add(Program.Transfer.Text("？？？", "「[P姓]……[P名前]よ……」"));
            list.Add(Program.Transfer.Text("[P名]", "「あ、あれ？ なんだ……？」"));
            list.Add(Program.Transfer.Create(false, Command.CharaCoordinate, "0", "Pajamas"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookNeck, "0", "3", "1"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookEyes, "0", "1"));
            list.Add(Program.Transfer.Expression("0", "標準"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotion, "0", "Stand_00_00"));
            list.Add(Program.Transfer.Create(false, Command.CharaActive, "0", "TRUE"));
            list.Add(Program.Transfer.Text("[P名]", "「だ……誰？」"));
            list.Add(Program.Transfer.Expression("0", "微笑"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotion, "0", "Stand_05_00"));
            list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d", "adm_03_08_007"));
            list.Add(Program.Transfer.Text("？？？", "「私はちかりん……コイカツを司る女神です……」"));
            list.Add(Program.Transfer.Text("[P名]", "「え……ち、ちかりん？ コイカツの女神？ なんですか、それ？」"));
            list.Add(Program.Transfer.Expression("0", "笑顔（目閉じ）"));
            list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d", "adm_02_08_001"));
            list.Add(Program.Transfer.Text("ちかりん", "「ふふふ……そのうち、信じるようになるでしょう……」"));
            list.Add(Program.Transfer.Text("[P名]", "「は、はあ……」"));
            list.Add(Program.Transfer.Expression("0", "ドヤ顔"));
            list.Add(Program.Transfer.Motion("0", "Stand_09_00"));
            list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d", "adm_03_08_015"));
            list.Add(Program.Transfer.Text("ちかりん", "「この学園にコイカツ部を創設した貴方には……『スティール』、女の子の着けているショーツを強奪する呪文を与えましょう……」"));
            list.Add(Program.Transfer.Text("[P名]", "（呪文って……この娘、頭大丈夫なのかな……）"));
            list.Add(Program.Transfer.Create(true, Command.SE2DPlay, "sound/data/systemse/00.unity3d", "sse_00_04", "0",
                "0", "TRUE", "TRUE", "-1", "FALSE", "FALSE"));
            list.Add(Program.Transfer.Create(false, Command.CharaActive, "0", "FALSE", "center"));
            list.Add(Program.Transfer.Text("[P名]", "「ええ！？ き……消えた……？ 本当に女神様……？」"));
            list.Add(Program.Transfer.Voice("0", "sound/data/pcm/c08/adm/00.unity3d", "adm_01_08_011"));
            list.Add(Program.Transfer.Text("ちかりん", "（まずは女の子の前でスティールと唱えるのです……）"));
            list.Add(Program.Transfer.Text("[P名]", "（こいつ……直接脳内に！）"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookNeck, "0", "0", "1"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookEyes, "0", "0"));
            list.Add(Program.Transfer.Close());
            return list;
        }

        private List<Program.Transfer> SpawnLevel1(SaveData.Player player)
        {
            var list = new List<Program.Transfer>();
            Program.SetParam(player, list);
            list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "ちかりん"));
            list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0", "white", "back", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.Filter, "back", "black", "0"));
            list.Add(Program.Transfer.Create(false, Command.MapChange, "保健室", "FALSE"));
            list.Add(Program.Transfer.Create(false, Command.BGMPlay, "HSceneGentle", "", "0", "", "TRUE"));
            list.Add(Program.Transfer.Create(true, Command.VAR, "System.Single", "FadeTime", "2"));
            list.Add(Program.Transfer.Create(false, Command.Filter, "back", "black", "0"));
            list.Add(Program.Transfer.Create(false, Command.ImageLoad, "adv/fade01.unity3d", "041", "", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.Fade, "in", "0", "black", "", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.NullLoad, "12", "保健室"));
            list.Add(Program.Transfer.Create(false, Command.EventCGSetting, "adv/eventcg/12.unity3d",
                "Health_HCamera_00"));
            list.Add(Program.Transfer.Create(false, Command.NullSet, "Health_HChara_00", "Chara"));
            list.Add(Program.Transfer.Create(true, Command.CharaMobCreate, "0", "custom/presets_f_00.unity3d",
                "ill_Default_Female"));
            list.Add(Program.Transfer.Create(false, Command.CharaCoordinate, "0", "Pajamas"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionIKSetPartner, "0", "-1"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionIKSetPartner, "-1", "0"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotion, "0", "WLoop", "h/anim/female/01_00_00.unity3d",
                "khh_f_base", "h/list/00_00.unity3d", "khh_f_08", "h/list/00_00.unity3d", "yure_khh_08_00",
                "h/anim/female/01_00_00.unity3d", "khh_f_08", "", "-1", "WLoop", "h/anim/male/01_00_00.unity3d",
                "khh_m_base", "h/list/00_00.unity3d", "khh_m_08", "", "", "h/anim/male/01_00_00.unity3d", "khh_m_08"));
            list.Add(Program.Transfer.Create(false, Command.CharaVoicePlay, "0", "Normal",
                "sound/data/pcm/c08/h/00_00.unity3d", "h_ko_08_00_037", "0", "0", "TRUE", "TRUE", "", "", "FALSE"));
            list.Add(Program.Transfer.Create(false, Command.CharaGetShape, "0", "HEIGHT", "0"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionSetParam, "0", "height", "HEIGHT"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionSetParam, "-1", "height", "HEIGHT"));
            list.Add(Program.Transfer.Create(false, Command.LookAtDankonAdd, "0", "h/list/", "dan_khh_08",
                "cm_J_dan101_00", "cm_J_dan109_00", "cm_J_dan100_00"));
            list.Add(Program.Transfer.Create(false, Command.HMotionShakeAdd));
            list.Add(Program.Transfer.Create(false, Command.CharaLookNeck, "0", "3", "1"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookNeck, "-1", "3", "1"));
            list.Add(Program.Transfer.Create(false, Command.CharaExpression, "0", "", "", "22", "", "", "1"));
            list.Add(Program.Transfer.Create(false, Command.CharaFixMouth, "0", "TRUE"));
            list.Add(Program.Transfer.Create(false, Command.CharaClothState, "-1", "bot", "2"));
            list.Add(Program.Transfer.Create(false, Command.CharaClothState, "-1", "shorts", "3"));
            list.Add(Program.Transfer.Create(false, Command.CharaVisible, "-1", "Head", "FALSE"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookEyes, "0", "1"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[P名]", "「すぅ……すぅ……ん、んん……」"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[P名]", "（ううん、何だか下半身に違和感が……？）"));
            list.Add(Program.Transfer.Create(false, Command.Wait, "0"));
            list.Add(Program.Transfer.Create(false, Command.Wait, "0"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[P名]", "「…………」"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[P名]", "「って……え？\u3000え？」"));
            list.Add(Program.Transfer.Create(false, Command.CharaVoiceStop, "0"));
            list.Add(Program.Transfer.Create(true, Command.CrossFade, "1"));
            list.Add(Program.Transfer.Create(true, Command.CharaFixMouth, "0", "FALSE"));
            list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stop_Idle", "", "", "", "", "", "", "",
                "", "", "-1", "Stop_Idle"));
            list.Add(Program.Transfer.Create(false, Command.CharaExpression, "0", "笑顔"));
            list.Add(Program.Transfer.Create(true, Command.Voice, "0", "sound/data/pcm/c00/adm/00.unity3d",
                "adm_00_00_010"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[ちかりん]", "「おはよ♡\u3000ぐっすり寝てたわね」"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[P名]", "「あの、ちかりんさんは何をして……？」"));
            list.Add(Program.Transfer.Create(true, Command.Voice, "0", "sound/data/pcm/c00/h/00_00.unity3d",
                "h_hh_-1_02_001"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[H名]", "「ふふっ、お疲れみたいだから、ちかりんがたっぷり尽くしてあげるわ」"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookNeck, "0", "0", "1"));
            list.Add(Program.Transfer.Create(false, Command.CharaLookEyes, "0", "0"));
            list.Add(Program.Transfer.Create(false, Command.EventCGRelease, "FALSE"));
            list.Add(Program.Transfer.Create(true, Command.VAR, "System.Int32", "nullNo", "12"));
            list.Add(Program.Transfer.Create(true, Command.VAR, "System.String", "hPos", "Health_HChara_00"));
            list.Add(Program.Transfer.Create(true, Command.VAR, "System.Int32", "appoint", "3"));
            list.Add(Program.Transfer.Create(false, Command.Close));
            return list;
        }

        private void LateUpdate()
        {
            if (_isDuringHScene || StudioAPI.InsideStudio || MakerAPI.InsideMaker) return;

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
                    StealPanty(talkScene);
                else
                    StartCoroutine(SpawnLevel());
            }
        }

        private void StealPanty(TalkScene talkScene)
        {
            Log("StealPanty");
            var playerController = Uty.GetPlayerController();
            Log($"StealLevel={playerController.Data.StealLevel}");
            if (playerController.Data.StealLevel == -1) return;
            var currentVisibleGirl = Uty.GetCurrentVisibleGirl();
            if (currentVisibleGirl == null || currentVisibleGirl.isTeacher ||
                currentVisibleGirl.schoolClass == -1) return;
            var coordinateType = currentVisibleGirl.chaCtrl.fileStatus.coordinateType;
            Log($"coodType={coordinateType}");
            var num = 3;
            var id = currentVisibleGirl.charFile.coordinate[coordinateType].clothes.parts[num].id;
            var baseColor = currentVisibleGirl.charFile.coordinate[coordinateType].clothes.parts[num].colorInfo[0]
                .baseColor;
            var pattern = currentVisibleGirl.charFile.coordinate[coordinateType].clothes.parts[num].colorInfo[0]
                .pattern;
            var patternColor = currentVisibleGirl.charFile.coordinate[coordinateType].clothes.parts[num].colorInfo[0]
                .patternColor;
            var listInfo = currentVisibleGirl.chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_shorts, id);
            if (id == 0 || listInfo == null)
            {
                Utils.Sound.Play(SystemSE.save);
                var obj = Language == 0
                    ? "残念、" + currentVisibleGirl.Name + "はノーパンだ！"
                    : "Unfortunately, " + currentVisibleGirl.firstname + " doesn't wear panties!";
                Caption.DisplayBlink(obj, Color.magenta, 0);
                Log(obj);
                return;
            }

            var num2 = 1;
            var id2 = currentVisibleGirl.charFile.coordinate[coordinateType].clothes.parts[num2].id;
            if (playerController.Data.StealLevel > 0 && id2 == 0 && WithoutBottoms.Value)
            {
                Utils.Sound.Play(SystemSE.save);
                var obj2 = Language == 0 ? "下半身丸出しにするのは可哀そうだな..." : "It seems pitiful to expose the lower body...";
                Caption.DisplayBlink(obj2, Color.magenta, 0);
                Log(obj2);
                return;
            }

            var num3 = 0f;
            if (currentVisibleGirl.isAnger)
            {
                num3 = 0f;
            }
            else if (AlwaysSuccessful.Value)
            {
                num3 = 100f;
            }
            else
            {
                if (currentVisibleGirl.isGirlfriend ||
                    currentVisibleGirl.HExperience == SaveData.Heroine.HExperienceKind.淫乱)
                    num3 = 80f;
                else if (currentVisibleGirl.HExperience == SaveData.Heroine.HExperienceKind.慣れ)
                    num3 = 60f;
                else if (currentVisibleGirl.HExperience == SaveData.Heroine.HExperienceKind.不慣れ)
                    num3 = 30f;
                else if (currentVisibleGirl.HExperience == SaveData.Heroine.HExperienceKind.初めて) num3 = 10f;
                if (currentVisibleGirl.parameter.attribute.bitch) num3 += 10f;
                if (currentVisibleGirl.parameter.attribute.choroi) num3 += 10f;
                if (currentVisibleGirl.parameter.attribute.donkan) num3 += 10f;
                if (currentVisibleGirl.personality == 19) num3 += 10f;
                if (currentVisibleGirl.personality == 18) num3 += 10f;
                if (currentVisibleGirl.personality == 24) num3 += 10f;
                if (currentVisibleGirl.personality == 13) num3 += 10f;
                if (currentVisibleGirl.personality == 0) num3 += 10f;
                if (currentVisibleGirl.personality == 11) num3 += 10f;
                if (currentVisibleGirl.personality == 33) num3 += 10f;
                if (currentVisibleGirl.personality == 2) num3 -= 10f;
                if (currentVisibleGirl.personality == 12) num3 -= 10f;
                if (currentVisibleGirl.personality == 14) num3 -= 10f;
                if (currentVisibleGirl.personality == 15) num3 -= 10f;
                if (currentVisibleGirl.personality == 17) num3 -= 10f;
                if (currentVisibleGirl.personality == 36) num3 -= 10f;
            }

            if (playerController.Data.StealLevel == 0) num3 = 100f;
            if (!Uty.Probability(num3))
            {
                Utils.Sound.Play(SystemSE.save);
                var obj3 = Language == 0 ? "スティール失敗！" : "Steel failed!";
                Caption.DisplayBlink(obj3, Color.blue, 0);
                Log(obj3);
                StartCoroutine(TalkGetAngry());
                return;
            }

            Utils.Sound.Play(SystemSE.ok_s);
            var msg = Language == 0
                ? currentVisibleGirl.Name + "の " + listInfo.Name + " をスティール！"
                : "Woo-hoo! Steeled " + currentVisibleGirl.firstname + "'s " + listInfo.Name + "!";
            var levelColor = new Color(1f, 0.68f, 0.78f, 1f);
            Instance.StartCoroutine(ShowMessage(msg, levelColor));
            Log(msg);
            try
            {
                var info = listInfo.GetInfo(ChaListDefine.KeyType.ThumbAB);
                var info2 = listInfo.GetInfo(ChaListDefine.KeyType.ThumbTex);
                var texture2D = CommonLib.LoadAsset<Texture2D>(info, info2, false, string.Empty);
                Wipe.DisplayCutIn(Wipe.CutInMode.Back2Front, new Texture2D[1]
                {
                    texture2D
                }, 0f, 0f, 0f, 0f, 20f);
            }
            catch (Exception arg)
            {
                Log($"GetTexture err={arg}");
            }

            if (DefaultOtherSteal != 0)
            {
                for (var i = 0; i <= 6; i++)
                {
                    if (i == coordinateType) continue;
                    if (DefaultOtherSteal == DefaultOtherSteal_EN.SamePantiesOnly)
                    {
                        var id3 = currentVisibleGirl.charFile.coordinate[i].clothes.parts[num].id;
                        var baseColor2 = currentVisibleGirl.charFile.coordinate[i].clothes.parts[num].colorInfo[0]
                            .baseColor;
                        var pattern2 = currentVisibleGirl.charFile.coordinate[i].clothes.parts[num].colorInfo[0]
                            .pattern;
                        var patternColor2 = currentVisibleGirl.charFile.coordinate[i].clothes.parts[num].colorInfo[0]
                            .patternColor;
                        if (id != id3 || baseColor != baseColor2 || pattern != pattern2 ||
                            patternColor != patternColor2) continue;
                    }

                    Uty.ApplyNoPanty(currentVisibleGirl, i, false);
                }

                currentVisibleGirl.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coordinateType,
                    false);
            }

            if (HalfOff.Value)
            {
                currentVisibleGirl.chaCtrl.fileStatus.clothesState[1] = 1;
                currentVisibleGirl.chaCtrl.fileStatus.clothesState[5] = 1;
            }

            Uty.ApplyNoPanty(currentVisibleGirl);
            PantyRobberGameController._NoPantyChara.Add(currentVisibleGirl);
            if (GirlReaction.Value) Uty.TouchMuneL(talkScene);
            if (playerController.Data.StealLevel == 0) StartCoroutine(TalkLevel());
            playerController.Data.StealLevel++;
            playerController.SaveData();
        }

        private IEnumerator TalkGetAngry()
        {
            var talkScene = FindObjectOfType<TalkScene>();
            var player = Uty.GetPlayer();
            var currentVisibleGirl = Uty.GetCurrentVisibleGirl();
            var senarioData = Uty.GetSenarioData(currentVisibleGirl, "42");
            if (senarioData == null) yield break;
            var list = new List<Program.Transfer>();
            Program.SetParam(player, currentVisibleGirl, list);
            list.Add(Program.Transfer.Text("[P名]", "「しまった！\u3000[H姓]に気づかれた！」"));
            foreach (var item in senarioData) list.Add(Program.Transfer.Create(item.Multi, item.Command, item.Args));
            list.Insert(list.Count - 3, Program.Transfer.Text("[P名]", "「ごめんごめん、しないから！（またしないとは言っていない）」"));
            Uty.StartADV(talkScene, list);
            yield return null;
            yield return Program.Wait("Talk");
            Observable.FromCoroutine(() => Uty.TalkEnd(talkScene)).Subscribe().AddTo(this);
        }

        private static IEnumerator ShowMessage(string msg, Color levelColor)
        {
            Caption.DisplayText(msg, levelColor, 6f, 0);
            yield return new WaitForSeconds(2f);
        }

        public static void Log(string msg, bool isErr = false)
        {
            if (isErr) Logger.LogError(msg);
        }

        public class Caption
        {
            public static bool ShowSubtitles = true;

            public static string FontName = "Arial";

            public static int FontSize = -4;

            public static FontStyle FontStyle = FontStyle.Bold;

            public static TextAnchor TextAlign = TextAnchor.LowerRight;

            public static int TextOffset = 800;

            public static int OutlineThickness = 2;

            private static Color co;

            public static Color TextColor = ColorUtility.TryParseHtmlString("#FFCCFFdd", out co) ? co : Color.magenta;

            public static Color OutlineColor = Color.black;

            internal static GameObject Pane { get; set; }

            public static void InitGUI()
            {
                if (!(Pane != null))
                {
                    Pane = new GameObject("KK_PantyRobber_Caption");
                    var obj = Pane.GetComponent<CanvasScaler>() ?? Pane.AddComponent<CanvasScaler>();
                    obj.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    obj.referenceResolution = new Vector2(1920f, 1080f);
                    obj.matchWidthOrHeight = 0.5f;
                    var obj2 = Pane.GetComponent<Canvas>() ?? Pane.AddComponent<Canvas>();
                    obj2.renderMode = RenderMode.ScreenSpaceOverlay;
                    obj2.sortingOrder = 500;
                    (Pane.GetComponent<CanvasGroup>() ?? Pane.AddComponent<CanvasGroup>()).blocksRaycasts = false;
                    var obj3 = Pane.GetComponent<VerticalLayoutGroup>() ?? Pane.AddComponent<VerticalLayoutGroup>();
                    obj3.childControlHeight = false;
                    obj3.childControlWidth = false;
                    obj3.childForceExpandHeight = false;
                    obj3.childForceExpandWidth = false;
                    obj3.childAlignment = TextAnchor.LowerCenter;
                    obj3.padding = new RectOffset(0, 100, 0, TextOffset);
                }
            }

            public static void DisplayBlink(string text, Color levelColor, int fsize, bool init = true)
            {
                if (!text.IsNullOrWhiteSpace() && (!(Pane == null) || init))
                    Instance.StartCoroutine(_DisplayBlink(text, levelColor, fsize, init));
            }

            private static IEnumerator _DisplayBlink(string text, Color levelColor, int fsize, bool init)
            {
                if (init)
                {
                    InitGUI();
                    yield return null;
                }

                var subtitle = new GameObject(text);
                var subtitleText = InitCaption(subtitle, levelColor, fsize);
                subtitleText.text = text;
                var BlinkColor = ColorUtility.TryParseHtmlString("#00000000", out co) ? co : Color.black;
                for (var i = 0; i < 12; i++)
                {
                    if (i % 2 == 0)
                        subtitleText.color = levelColor;
                    else
                        subtitleText.color = BlinkColor;
                    yield return new WaitForSeconds(0.25f);
                }

                subtitle.transform.SetParent(null);
                Destroy(subtitle);
            }

            public static void DisplayText(string text, Color dispColor, float waitSec, int fsize, bool init = true)
            {
                if (!text.IsNullOrWhiteSpace() && (!(Pane == null) || init))
                    Instance.StartCoroutine(_DisplayText(text, dispColor, waitSec, fsize, init));
            }

            private static IEnumerator _DisplayText(string text, Color dispColor, float waitSec, int fsize, bool init)
            {
                if (init)
                {
                    InitGUI();
                    yield return null;
                }

                var subtitle = new GameObject(text);
                InitCaption(subtitle, dispColor, fsize).text = text;
                yield return new WaitForSeconds(waitSec);
                subtitle.transform.SetParent(null);
                Destroy(subtitle);
            }

            private static Text InitCaption(GameObject subtitle, Color levelColor, int fsize)
            {
                var font = (Font)Resources.GetBuiltinResource(typeof(Font), FontName + ".ttf");
                if (fsize == 0) fsize = FontSize;
                fsize = (int)(fsize < 0 ? fsize * Screen.height / -100.0 : (double)fsize);
                subtitle.transform.SetParent(Pane.transform);
                var obj = subtitle.GetComponent<RectTransform>() ?? subtitle.AddComponent<RectTransform>();
                obj.pivot = new Vector2(0.5f, 0f);
                obj.sizeDelta = new Vector2(Screen.width * 0.99f, fsize + fsize * 0.05f);
                var obj2 = subtitle.GetComponent<Text>() ?? subtitle.AddComponent<Text>();
                obj2.font = font;
                obj2.fontSize = fsize;
                obj2.fontStyle = font.dynamic ? FontStyle : FontStyle.Normal;
                obj2.alignment = TextAlign;
                obj2.horizontalOverflow = HorizontalWrapMode.Wrap;
                obj2.verticalOverflow = VerticalWrapMode.Overflow;
                obj2.color = levelColor;
                var obj3 = subtitle.GetComponent<Outline>() ?? subtitle.AddComponent<Outline>();
                obj3.effectColor = OutlineColor;
                obj3.effectDistance = new Vector2(OutlineThickness, OutlineThickness);
                return obj2;
            }
        }
    }
}