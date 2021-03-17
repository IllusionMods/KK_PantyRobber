using System;
using System.Collections;
using Illusion.Game;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KK_PantyRobber
{
    public class Wipe
    {
        public enum CutInMode
        {
            None,
            Right2Left,
            Left2Right,
            Top2Down,
            Down2Top,
            Back2Front,
            Front2Back
        }

        internal static DefaultControls.Resources resources;

        internal static GameObject Pane { get; set; }

        public static void DisplayCutIn(CutInMode mode, Texture2D[] frames, float firstWait, float secondTimer,
            float hoseiX, float hoseiY, float hoseiZ, SystemSE secondSound = SystemSE.sel, bool txDestroy = false)
        {
            if (Pane == null) Pane = new GameObject("KK_PantyRobber_Wipe");
            var obj = Pane.GetComponent<CanvasScaler>() ?? Pane.AddComponent<CanvasScaler>();
            obj.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            obj.referenceResolution = new Vector2(Screen.width, Screen.height);
            obj.matchWidthOrHeight = 0.5f;
            var canvas = Pane.GetComponent<Canvas>() ?? Pane.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            (Pane.GetComponent<CanvasGroup>() ?? Pane.AddComponent<CanvasGroup>()).blocksRaycasts = false;
            PantyRobber.Log("DisplayCutIn");
            PantyRobber.Instance.StartCoroutine(_DisplayCutIn(canvas, mode, frames, firstWait, secondTimer, hoseiX,
                hoseiY, hoseiZ, secondSound, txDestroy));
        }

        internal static IEnumerator _DisplayCutIn(Canvas canvas, CutInMode mode, Texture2D[] frames, float firstWait,
            float secondTimer, float hoseiX, float hoseiY, float hoseiZ, SystemSE secondSound = SystemSE.sel,
            bool txDestroy = false)
        {
            PantyRobber.Log("_DisplayCutIn");
            var gameObject = new GameObject();
            gameObject.transform.SetParent(canvas.transform, false);
            gameObject.SetActive(false);
            var cutIn = new cutIn();
            cutIn.txDestroy = txDestroy;
            cutIn.mode = mode;
            cutIn.secondTimer = secondTimer;
            cutIn.secondSound = secondSound;
            cutIn.frames = frames;
            cutIn.video = CreateRawImage("", gameObject.transform, null);
            cutIn.video.rectTransform.sizeDelta =
                new Vector2(Screen.height * (hoseiZ / 100f), Screen.height * (hoseiZ / 100f));
            cutIn.hoseiX = Screen.width / 2f * (hoseiX / 100f);
            cutIn.hoseiY = Screen.height / 2f * (hoseiY / 100f);
            PantyRobber.Log($"cutIn.frames.Length={cutIn.frames.Length}");
            yield return new WaitForSeconds(firstWait);
            if (mode == CutInMode.None)
            {
                cutIn.alpha = 1f;
                cutIn.waitTimer = secondTimer;
            }
            else
            {
                firstMode(cutIn);
                cutIn.waitTimer = 0f;
            }

            gameObject.SetActive(true);
            while (true)
            {
                try
                {
                    var num = (int)(Time.time * cutIn.framesPerSecond);
                    num %= cutIn.frames.Length;
                    cutIn.video.texture = cutIn.frames[num];
                    if (cutIn.waitTimer > 0f)
                    {
                        cutIn.waitTimer -= Time.deltaTime;
                    }
                    else
                    {
                        if (mode == CutInMode.None)
                        {
                            if (cutIn.txDestroy && cutIn.frames != null)
                            {
                                var frames2 = cutIn.frames;
                                for (var i = 0; i < frames2.Length; i++) Object.Destroy(frames2[i]);
                                cutIn.frames = null;
                            }

                            if (null != cutIn.Transform.parent.gameObject)
                            {
                                PantyRobber.Log("GameObject.Destroy");
                                Object.Destroy(cutIn.Transform.parent.gameObject);
                                cutIn.video = null;
                                cutIn = null;
                            }

                            yield break;
                        }

                        cutIn.alpha = Mathf.SmoothDamp(cutIn.alpha, cutIn.targetAlpha, ref cutIn.velocityAlpha,
                            cutIn.smoothTime);
                        cutIn.Transform.localScale = Vector3.SmoothDamp(cutIn.Transform.localScale, cutIn.targetScale,
                            ref cutIn.velocity2, cutIn.smoothTime);
                        cutIn.Transform.position = Vector3.SmoothDamp(cutIn.Transform.position, cutIn.targetPosition,
                            ref cutIn.velocity, cutIn.smoothTime);
                        if ((cutIn.Transform.position - cutIn.targetPosition).sqrMagnitude < 1f && NextMode(cutIn))
                            yield break;
                    }
                }
                catch (Exception arg)
                {
                    PantyRobber.Log($"_DisplayCutIn ERR={arg}");
                }

                yield return null;
            }
        }

        private static RawImage CreateRawImage(string objectName, Transform p, Texture texture)
        {
            var gameObject2 = DefaultControls.CreateRawImage(resources);
            gameObject2.name = objectName;
            gameObject2.transform.SetParent(p, false);
            var component = gameObject2.GetComponent<RawImage>();
            component.texture = texture;
            return component;
        }

        internal static void firstMode(cutIn cutIn)
        {
            PantyRobber.Log($"firstMode mode={cutIn.mode}");
            try
            {
                switch (cutIn.mode)
                {
                    case CutInMode.None:
                        cutIn.Transform.position = new Vector3(Screen.width + cutIn.Width / 2f + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        break;
                    case CutInMode.Right2Left:
                        cutIn.Transform.position = new Vector3(Screen.width + cutIn.Width / 2f + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.alpha = -2f;
                        cutIn.targetAlpha = 1f;
                        break;
                    case CutInMode.Left2Right:
                        cutIn.Transform.position = new Vector3(-1f * (Screen.width + cutIn.Width / 2f) + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.alpha = -2f;
                        cutIn.targetAlpha = 1f;
                        break;
                    case CutInMode.Top2Down:
                        cutIn.Transform.position = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height + cutIn.Height / 2f + cutIn.hoseiY);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.alpha = -2f;
                        cutIn.targetAlpha = 1f;
                        break;
                    case CutInMode.Down2Top:
                        cutIn.Transform.position = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            -1f * (Screen.height + cutIn.Height / 2f) + cutIn.hoseiY);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.alpha = -2f;
                        cutIn.targetAlpha = 1f;
                        break;
                    case CutInMode.Back2Front:
                        cutIn.Transform.position = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY, Screen.width / 2);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY, 0f);
                        cutIn.Transform.localScale = new Vector3(0.1f, 0.1f);
                        cutIn.targetScale = Vector3.one;
                        cutIn.alpha = -2f;
                        cutIn.targetAlpha = 1f;
                        break;
                    case CutInMode.Front2Back:
                        cutIn.Transform.position = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY, Screen.width / 2);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY, 0f);
                        cutIn.Transform.localScale = new Vector3(5f, 5f);
                        cutIn.targetScale = Vector3.one;
                        cutIn.alpha = -2f;
                        cutIn.targetAlpha = 1f;
                        break;
                }
            }
            catch (Exception arg)
            {
                PantyRobber.Log($"firstMode ERR={arg}");
            }
        }

        internal static bool NextMode(cutIn cutIn)
        {
            PantyRobber.Log($"NextMode mode={cutIn.mode}");
            try
            {
                if (cutIn.mode != 0 && cutIn.secondSound != 0) Utils.Sound.Play(cutIn.secondSound);
                switch (cutIn.mode)
                {
                    case CutInMode.None:
                        if (cutIn.txDestroy && cutIn.frames != null)
                        {
                            var frames = cutIn.frames;
                            for (var i = 0; i < frames.Length; i++) Object.Destroy(frames[i]);
                            cutIn.frames = null;
                        }

                        if (null != cutIn.Transform.parent.gameObject)
                        {
                            PantyRobber.Log("GameObject.Destroy");
                            Object.Destroy(cutIn.Transform.parent.gameObject);
                            cutIn.video = null;
                            cutIn = null;
                        }

                        return true;
                    case CutInMode.Right2Left:
                        cutIn.waitTimer = cutIn.secondTimer;
                        cutIn.alpha = 1f;
                        cutIn.targetAlpha = -2f;
                        cutIn.targetPosition = new Vector3(0f - cutIn.Width / 2f + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.mode = CutInMode.None;
                        break;
                    case CutInMode.Left2Right:
                        cutIn.waitTimer = cutIn.secondTimer;
                        cutIn.alpha = 1f;
                        cutIn.targetAlpha = -2f;
                        cutIn.targetPosition = new Vector3(Screen.width + cutIn.Width / 2f + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY);
                        cutIn.mode = CutInMode.None;
                        break;
                    case CutInMode.Top2Down:
                        cutIn.waitTimer = cutIn.secondTimer;
                        cutIn.alpha = 1f;
                        cutIn.targetAlpha = -2f;
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            0f - cutIn.Width / 2f + cutIn.hoseiY);
                        cutIn.mode = CutInMode.None;
                        break;
                    case CutInMode.Down2Top:
                        cutIn.waitTimer = cutIn.secondTimer;
                        cutIn.alpha = 1f;
                        cutIn.targetAlpha = -2f;
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height + cutIn.Height / 2f + cutIn.hoseiY);
                        cutIn.mode = CutInMode.None;
                        break;
                    case CutInMode.Back2Front:
                        cutIn.waitTimer = cutIn.secondTimer;
                        cutIn.alpha = 1f;
                        cutIn.targetAlpha = -2f;
                        cutIn.targetScale = new Vector3(5f, 5f);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY, -Screen.width / 2);
                        cutIn.mode = CutInMode.None;
                        break;
                    case CutInMode.Front2Back:
                        cutIn.waitTimer = cutIn.secondTimer;
                        cutIn.alpha = 1f;
                        cutIn.targetAlpha = -2f;
                        cutIn.targetScale = new Vector3(0.1f, 0.1f);
                        cutIn.targetPosition = new Vector3(Screen.width / 2 + cutIn.hoseiX,
                            Screen.height / 2 + cutIn.hoseiY, -Screen.width / 2);
                        cutIn.mode = CutInMode.None;
                        break;
                }
            }
            catch (Exception arg)
            {
                PantyRobber.Log($"NextMode ERR={arg}");
            }

            return false;
        }

        internal class cutIn
        {
            internal float _alpha;

            internal Texture2D[] frames;

            internal float framesPerSecond = 10f;

            internal float hoseiX;

            internal float hoseiY;
            internal CutInMode mode;

            internal SystemSE secondSound;

            internal float secondTimer;

            internal float smoothTime = 0.3f;

            internal float targetAlpha;

            internal Vector3 targetPosition = Vector3.zero;

            internal Vector3 targetScale = Vector3.one;

            internal bool txDestroy;

            internal Vector3 velocity = Vector3.zero;

            internal Vector3 velocity2 = Vector3.zero;

            internal float velocityAlpha;

            internal RawImage video;

            internal float waitTimer;

            internal float Width
            {
                get
                {
                    if (video == null || video.texture == null) return 0f;
                    return video.texture.width;
                }
            }

            internal float Height
            {
                get
                {
                    if (video == null || video.texture == null) return 0f;
                    return video.texture.height;
                }
            }

            internal Transform Transform => video?.transform;

            internal float alpha
            {
                get => _alpha;
                set
                {
                    _alpha = value;
                    if (video != null && video.texture != null)
                        video.color = new Color(1f, 1f, 1f, Mathf.Clamp01(_alpha));
                }
            }
        }
    }
}