using System;
using System.Collections;
using System.Collections.Generic;
using ActionGame.Chara;
using ADV;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace KK_PantyRobber
{
    internal class TalkLevelSpawner
    {
        private static IEnumerator TalkLevel()
        {
            var talkScene = Object.FindObjectOfType<TalkScene>();
            if (talkScene == null) yield break;
            var player = Uty.GetPlayer();
            if (player != null && Uty.GetCurrentVisibleGirl() != null)
            {
                var list = new List<Program.Transfer>();
                Program.SetParam(player, list);
                list.Add(Program.Transfer.Create(false, Command.FontColor, "Color2", "ちかりん"));
                list.Add(Program.Transfer.Text("[P名]", "「スティール！」"));
                list.Add(Program.Transfer.Text("[P名]", "（わっ、本当にできた……）"));
                list.Add(Program.Transfer.Create(true, Command.SE2DPlay, "sound/data/pcm/c08/adm/00.unity3d", "adm_00_08_031_04", "0", "0", "TRUE", "TRUE", "-1", "FALSE", "FALSE"));
                list.Add(Program.Transfer.Text("ちかりん", "（その調子です……もっともっとスティールするのです……）"));
                list.Add(Program.Transfer.Close());
                Uty.StartADV(talkScene, list);
                yield return null;
                yield return Program.Wait("Talk");
                Observable.FromCoroutine(() => Uty.TalkEnd(talkScene)).Subscribe().AddTo(PantyRobber.Instance);
            }
        }

        public static IEnumerator SpawnLevel()
        {
            PantyRobber.Log("SpawnLevel");
            var actScene = Singleton<Game>.Instance.actScene;
            var player = Uty.GetPlayer();
            if (player == null) yield break;
            var playerStatus = Uty.GetPlayerController();
            PantyRobber.Log($"StealLevel={playerStatus.Data.StealLevel}");
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
                        var array = new[]
                        {
                        "adm_00_08_003",
                        "adm_00_08_005",
                        "adm_00_08_007",
                        "adm_00_08_016",
                        "adm_00_08_021",
                        "adm_01_08_004"
                    };
                        var array2 = new[]
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
            var prevBgm = string.Empty;
            var prevVolume = 1f;
            if (!dialogOnly)
            {
                actScene.SetProperty("_isInChargeBGM", true);
                actScene.Player.isActionNow = true;
                actScene.SetProperty("isEventNow", true);
                actScene.Player.isLesMotionPlay = false;
                actScene.SetProperty("shortcutKey", false);
                yield return PantyRobber.Instance.StartCoroutine(Utils.Sound.GetBGMandVolume(delegate (string bgm, float volume)
                {
                    prevBgm = bgm;
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
            var isOpenAdv = false;
            yield return PantyRobber.Instance.StartCoroutine(Program.Open(new Data
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
                onLoad = delegate { isOpenAdv = true; }
            }));
            yield return new WaitUntil(() => isOpenAdv);
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
                    yield return PantyRobber.Instance.StartCoroutine(Singleton<Scene>.Instance.Fade(SimpleFade.Fade.Out));
                yield return PantyRobber.Instance.StartCoroutine(Utils.Sound.GetFadePlayerWhileNull(prevBgm, prevVolume));
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

        private static List<Program.Transfer> SpawnLevel0(SaveData.Player player)
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
            list.Add(Program.Transfer.Create(true, Command.CharaMobCreate, "0", "custom/presets_f_00.unity3d", "ill_Default_Female"));
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
            list.Add(Program.Transfer.Create(true, Command.SE2DPlay, "sound/data/systemse/00.unity3d", "sse_00_04", "0", "0", "TRUE", "TRUE", "-1", "FALSE", "FALSE"));
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

        private static List<Program.Transfer> SpawnLevel1(SaveData.Player player) //todo
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
            list.Add(Program.Transfer.Create(false, Command.EventCGSetting, "adv/eventcg/12.unity3d", "Health_HCamera_00"));
            list.Add(Program.Transfer.Create(false, Command.NullSet, "Health_HChara_00", "Chara"));
            list.Add(Program.Transfer.Create(true, Command.CharaMobCreate, "0", "custom/presets_f_00.unity3d", "ill_Default_Female"));
            list.Add(Program.Transfer.Create(false, Command.CharaCoordinate, "0", "Pajamas"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionIKSetPartner, "0", "-1"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionIKSetPartner, "-1", "0"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotion, "0", "WLoop", "h/anim/female/01_00_00.unity3d", "khh_f_base", "h/list/00_00.unity3d", "khh_f_08", "h/list/00_00.unity3d", "yure_khh_08_00", "h/anim/female/01_00_00.unity3d", "khh_f_08", "", "-1", "WLoop", "h/anim/male/01_00_00.unity3d", "khh_m_base", "h/list/00_00.unity3d", "khh_m_08", "", "", "h/anim/male/01_00_00.unity3d", "khh_m_08"));
            list.Add(Program.Transfer.Create(false, Command.CharaVoicePlay, "0", "Normal", "sound/data/pcm/c08/h/00_00.unity3d", "h_ko_08_00_037", "0", "0", "TRUE", "TRUE", "", "", "FALSE"));
            list.Add(Program.Transfer.Create(false, Command.CharaGetShape, "0", "HEIGHT", "0"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionSetParam, "0", "height", "HEIGHT"));
            list.Add(Program.Transfer.Create(false, Command.CharaMotionSetParam, "-1", "height", "HEIGHT"));
            list.Add(Program.Transfer.Create(false, Command.LookAtDankonAdd, "0", "h/list/", "dan_khh_08", "cm_J_dan101_00", "cm_J_dan109_00", "cm_J_dan100_00"));
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
            list.Add(Program.Transfer.Create(true, Command.CharaMotion, "0", "Stop_Idle", "", "", "", "", "", "", "", "", "", "-1", "Stop_Idle"));
            list.Add(Program.Transfer.Create(false, Command.CharaExpression, "0", "笑顔"));
            list.Add(Program.Transfer.Create(true, Command.Voice, "0", "sound/data/pcm/c00/adm/00.unity3d", "adm_00_00_010"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[ちかりん]", "「おはよ♡\u3000ぐっすり寝てたわね」"));
            list.Add(Program.Transfer.Create(false, Command.Text, "[P名]", "「あの、ちかりんさんは何をして……？」"));
            list.Add(Program.Transfer.Create(true, Command.Voice, "0", "sound/data/pcm/c00/h/00_00.unity3d", "h_hh_-1_02_001"));
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

        public static bool StealPanty(TalkScene talkScene, SaveData.Heroine targetGirl)
        {
            PantyRobber.Log("StealPanty");
            var playerController = Uty.GetPlayerController();
            PantyRobber.Log($"StealLevel={playerController.Data.StealLevel}");
            if (playerController.Data.StealLevel == -1) return false;
            if (targetGirl == null || targetGirl.isTeacher || targetGirl.schoolClass == -1) return false;

            var chaCtrl = targetGirl.chaCtrl;
            var coordinateType = chaCtrl.fileStatus.coordinateType;
            PantyRobber.Log($"coodType={coordinateType}");

            const int pantsPartIndex = 3;
            var coordinate = targetGirl.charFile.coordinate[coordinateType];
            var coordPart = coordinate.clothes.parts[pantsPartIndex];
            var id = coordPart.id;
            var listInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_shorts, id);
            if (id == 0 || listInfo == null)
            {
                Utils.Sound.Play(SystemSE.save);
                var obj = PantyRobber.Language == 0
                    ? "残念、" + targetGirl.Name + "はノーパンだ！"
                    : "Unfortunately, " + targetGirl.firstname + " doesn't wear panties!";
                Caption.DisplayBlink(obj, Color.magenta, 0);
                PantyRobber.Log(obj);
                return false;
            }

            var num2 = 1;
            var id2 = coordinate.clothes.parts[num2].id;
            if (playerController.Data.StealLevel > 0 && id2 == 0 && PantyRobber.WithoutBottoms.Value)
            {
                Utils.Sound.Play(SystemSE.save);
                var obj2 = PantyRobber.Language == 0 ? "下半身丸出しにするのは可哀そうだな..." : "It seems pitiful to expose the lower body...";
                Caption.DisplayBlink(obj2, Color.magenta, 0);
                PantyRobber.Log(obj2);
                return false;
            }

            var successProbability = 0f;
            if (playerController.Data.StealLevel == 0) //todo why?
            {
                successProbability = 100f;
            }
            else if (targetGirl.isAnger)
            {
                successProbability = 0f;
            }
            else if (PantyRobber.AlwaysSuccessful.Value)
            {
                successProbability = 100f;
            }
            else
            {
                if (targetGirl.isGirlfriend || targetGirl.HExperience == SaveData.Heroine.HExperienceKind.淫乱) successProbability = 80f;
                else if (targetGirl.HExperience == SaveData.Heroine.HExperienceKind.慣れ) successProbability = 60f;
                else if (targetGirl.HExperience == SaveData.Heroine.HExperienceKind.不慣れ) successProbability = 30f;
                else if (targetGirl.HExperience == SaveData.Heroine.HExperienceKind.初めて) successProbability = 10f;

                if (targetGirl.parameter.attribute.bitch) successProbability += 10f;
                if (targetGirl.parameter.attribute.choroi) successProbability += 10f;
                if (targetGirl.parameter.attribute.donkan) successProbability += 10f;

                if (targetGirl.personality == 19) successProbability += 10f;
                else if (targetGirl.personality == 18) successProbability += 10f;
                else if (targetGirl.personality == 24) successProbability += 10f;
                else if (targetGirl.personality == 13) successProbability += 10f;
                else if (targetGirl.personality == 0) successProbability += 10f;
                else if (targetGirl.personality == 11) successProbability += 10f;
                else if (targetGirl.personality == 33) successProbability += 10f;
                else if (targetGirl.personality == 2) successProbability -= 10f;
                else if (targetGirl.personality == 12) successProbability -= 10f;
                else if (targetGirl.personality == 14) successProbability -= 10f;
                else if (targetGirl.personality == 15) successProbability -= 10f;
                else if (targetGirl.personality == 17) successProbability -= 10f;
                else if (targetGirl.personality == 36) successProbability -= 10f;
            }

            if (!Uty.Probability(successProbability))
            {
                Utils.Sound.Play(SystemSE.save);
                var obj3 = PantyRobber.Language == 0 ? "スティール失敗！" : "Steel failed!";
                Caption.DisplayBlink(obj3, Color.blue, 0);
                PantyRobber.Log(obj3);
                PantyRobber.Instance.StartCoroutine(TalkGetAngry());
                return false;
            }

            Utils.Sound.Play(SystemSE.ok_s);
            var msg = PantyRobber.Language == 0
                ? targetGirl.Name + "の " + listInfo.Name + " をスティール！"
                : "Woo-hoo! Steeled " + targetGirl.firstname + "'s " + listInfo.Name + "!";
            var levelColor = new Color(1f, 0.68f, 0.78f, 1f);
            PantyRobber.Instance.StartCoroutine(ShowMessage(msg, levelColor));
            PantyRobber.Log(msg);
            try
            {
                var info = listInfo.GetInfo(ChaListDefine.KeyType.ThumbAB);
                var info2 = listInfo.GetInfo(ChaListDefine.KeyType.ThumbTex);
                var texture2D = CommonLib.LoadAsset<Texture2D>(info, info2, false, string.Empty);
                Wipe.DisplayCutIn(Wipe.CutInMode.Back2Front, new[] { texture2D }, 0f, 0f, 0f, 0f, 20f);
            }
            catch (Exception arg)
            {
                PantyRobber.Log($"GetTexture err={arg}");
            }

            if (PantyRobber.DefaultOtherSteal != 0)
            {
                var currentColor = coordPart.colorInfo[0].baseColor;
                var currentPattern = coordPart.colorInfo[0].pattern;
                var currentPatternColor = coordPart.colorInfo[0].patternColor;

                // Steal the panty from other coordinates
                for (var i = 0; i <= 6; i++)
                {
                    if (i == coordinateType) continue;

                    if (PantyRobber.DefaultOtherSteal == PantyRobber.DefaultOtherSteal_EN.SamePantiesOnly)
                    {
                        // Check if the panty is the same as the current coordinate, if no then skip
                        var otherClothes = targetGirl.charFile.coordinate[i].clothes.parts[pantsPartIndex];
                        var otherColorInfo = otherClothes.colorInfo[0];
                        if (id != otherClothes.id || currentColor != otherColorInfo.baseColor || currentPattern != otherColorInfo.pattern || currentPatternColor != otherColorInfo.patternColor)
                            continue;
                    }

                    Uty.ApplyNoPanty(targetGirl, i, false);
                }

                chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coordinateType,
                    false);
            }

            if (PantyRobber.HalfOff.Value)
            {
                chaCtrl.fileStatus.clothesState[1] = 1;
                chaCtrl.fileStatus.clothesState[5] = 1;
            }

            Uty.ApplyNoPanty(targetGirl);
            if (PantyRobber.GirlReaction.Value) Uty.TouchMuneL(talkScene);
            if (playerController.Data.StealLevel == 0) PantyRobber.Instance.StartCoroutine(TalkLevel());
            playerController.Data.StealLevel++;
            playerController.SaveData();

            return true;
        }

        private static IEnumerator TalkGetAngry()
        {
            var talkScene = Object.FindObjectOfType<TalkScene>();
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
            Observable.FromCoroutine(() => Uty.TalkEnd(talkScene)).Subscribe().AddTo(PantyRobber.Instance);
        }

        private static IEnumerator ShowMessage(string msg, Color levelColor)
        {
            Caption.DisplayText(msg, levelColor, 6f, 0);
            yield return new WaitForSeconds(2f);
        }
    }
}