using System.Collections;
using System.Collections.Generic;
using ActionGame;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using UnityEngine;

namespace KK_PantyRobber
{
    internal class PantyRobberGameController : GameCustomFunctionController
    {
        public static readonly List<SaveData.Heroine> _NoPantyChara = new List<SaveData.Heroine>();

        protected override void OnDayChange(Cycle.Week day)
        {
        }

        protected override void OnPeriodChange(Cycle.Type period)
        {
        }

        internal void OnSceneUnload(SaveData.Heroine heroine, ChaControl controller)
        {
            if (!StudioAPI.InsideStudio && !MakerAPI.InsideMaker && PantyRobber.EnablePantyRobber.Value)
            {
                PantyRobber.Log("OnSceneUnload");
                StartCoroutine(RefreshOnSceneChangeCo(heroine, false));
            }
        }

        private static IEnumerator RefreshOnSceneChangeCo(SaveData.Heroine girl, bool afterH)
        {
            var girl2 = girl;
            if (_NoPantyChara.Contains(girl2))
            {
                PantyRobber.Log("RefreshOnSceneChangeCo");
                var previousControl = girl2.chaCtrl;
                yield return new WaitUntil(() => girl2.chaCtrl != previousControl && girl2.chaCtrl != null);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                _NoPantyChara.Remove(girl2);
                Uty.ApplyNoPanty(girl2);
                if (girl2.chaCtrl.fileParam.sex == 1) girl2.chaCtrl.fileStatus.visibleSonAlways = false;
            }
        }

        protected override void OnStartH(HSceneProc proc, bool freeH)
        {
            PantyRobber._isDuringHScene = true;
        }

        protected override void OnEndH(HSceneProc proc, bool freeH)
        {
            PantyRobber._isDuringHScene = false;
            foreach (var item in proc.flags.lstHeroine) StartCoroutine(RefreshOnSceneChangeCo(item, true));
        }
    }
}