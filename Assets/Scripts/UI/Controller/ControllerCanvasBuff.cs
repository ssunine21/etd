using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasBuff : ControllerCanvas
    {
        private ViewCanvasBuff View => ViewCanvas as ViewCanvasBuff;
        private const int BuffDurationTimePerMinutes = 20;

        public ControllerCanvasBuff(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasBuff>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);

            for (var i = 0; i < View.ViewSlotBuffs.Length; ++i)
            {
                var index = i;
                View.ViewSlotBuffs[index]
                    .IconAnimation()
                    .WatchAdsButton.onClick.AddListener(() =>
                    {
                        if (DataController.Instance.buff.IsFree(index))
                        {
                            BuffOn(index, true);
                            DataController.Instance.buff.UseFree(index);
                        }
                        else
                        {
                            GoogleMobileAdsManager.Instance.ShowRewardedAd(() => BuffOn(index, true));
                        }
                    });
            }
            
            TimeTask().Forget();
        }

        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            UpdateAllSlot();
            
            while (!Cts.IsCancellationRequested)
            {
                for (var i = 0; i < View.ViewSlotBuffs.Length; ++i)
                {
                    if (DataController.Instance.buff.IsRemainTime(i))
                    {
                        DataController.Instance.buff.DecreaseRemainTime(i);
                        UpdateTime(i);
                    }
                    else
                    {
                        BuffOn(i, false);
                    }
                }
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
            
        }

        private void BuffOn(int index, bool flag)
        {
            if (DataController.Instance.buff.IsBuffOn(index) == flag) return;
            
            if (flag)
            {
                DataController.Instance.buff.SetRemainTime(index, 60 * BuffDurationTimePerMinutes);
                IncreaseExp(index, DataController.Instance.buff.GetIncreaseExp(index));
                if(index == 3) DataController.Instance.quest.Count(QuestType.ClickGameSpeedBuff);
            }
            else
            {
                if (DataController.Instance.good.GetValue(GoodType.RemoveAds) > 0)
                {
                    BuffOn(index, true);
                }
            }
            
            DataController.Instance.buff.SetBuffOn(index, flag);
            UpdateSlot(index);
        }

        private void IncreaseExp(int index, int amount)
        {
            var bBuff = DataController.Instance.buff;
            var postExp = bBuff.GetCurrExp(index) + amount;
            
            if (postExp >= bBuff.GetMaxExp(index))
            {
                postExp = 0;
                bBuff.SetLevel(index, bBuff.GetLevel(index) + 1);
            }
            
            bBuff.SetCurrExp(index, postExp);
        }

        private void UpdateAllSlot()
        {
            for (var i = 0; i < View.ViewSlotBuffs.Length; ++i)
            {
                UpdateSlot(i);
            }
        }

        private void UpdateSlot(int index)
        {
            var bBuff = DataController.Instance.buff;
            var isBlock = DataController.Instance.buff.isBuffOn.Take(DataController.Instance.buff.isBuffOn.Count - 1).Any(isOn => !isOn);
            View.ViewSlotBuffs[3].SetBlock(isBlock);
            View.ViewSlotBuffs[3].WatchAdsButton.enabled = !bBuff.IsRemainTime(3) && !isBlock;

            View.ViewSlotBuffs[index]
                .SetLevel(bBuff.GetLevel(index) + 1)
                .SetExp(bBuff.GetCurrExp(index), bBuff.GetMaxExp(index))
                .SetContents($"{LocalizeManager.GetText(bBuff.GetAttributeType(index))}\n" +
                             $"<b><size=+15><color=orange>" +
                             $"{bBuff.GetValue(index).ToAttributeValueString(bBuff.GetAttributeType(index))}" +
                             $"</color></size></b>")
                .SetFreeOnceButton(bBuff.IsFree(index))
                .SetOn(bBuff.IsRemainTime(index));

            if (index < 3)
                View.ViewSlotBuffs[index].WatchAdsButton.enabled = !bBuff.IsRemainTime(index);
            
            
            UpdateTime(index);
            
            DataController.Instance.buff.OnBindUpdateBuff?.Invoke();
            DataController.Instance.buff.OnBindGameSpeed?.Invoke();
            
            View.SetTotalGameSpeedText(LocalizeManager.GetText(LocalizedTextType.TotalGameSpeedDesc, Time.timeScale));
        }

        private void UpdateTime(int index)
        {
            var remainTime = DataController.Instance.buff.GetRemainTime(index);
            var minutes = $"{remainTime / 60:D2}";
            var seconds = $"{remainTime % 60:D2}";
            View.ViewSlotBuffs[index].SetRemainTime($"{minutes}:{seconds}");
        }
    }
}