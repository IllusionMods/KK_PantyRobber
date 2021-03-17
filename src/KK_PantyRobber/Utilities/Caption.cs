using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KK_PantyRobber
{
    internal class Caption
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
                PantyRobber.Instance.StartCoroutine(_DisplayBlink(text, levelColor, fsize, init));
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
            var blinkColor = ColorUtility.TryParseHtmlString("#00000000", out co) ? co : Color.black;
            for (var i = 0; i < 12; i++)
            {
                subtitleText.color = i % 2 == 0 ? levelColor : blinkColor;
                yield return new WaitForSeconds(0.25f);
            }

            subtitle.transform.SetParent(null);
            Object.Destroy(subtitle);
        }

        public static void DisplayText(string text, Color dispColor, float waitSec, int fsize, bool init = true)
        {
            if (!text.IsNullOrWhiteSpace() && (!(Pane == null) || init))
                PantyRobber.Instance.StartCoroutine(_DisplayText(text, dispColor, waitSec, fsize, init));
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
            Object.Destroy(subtitle);
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