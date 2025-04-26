using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasOfflineReward : ControllerCanvas
    {
        private const string ViewSlotRewardName = "ViewSlotGoodSquare";
        
        private ViewCanvasOfflineReward View => ViewCanvas as ViewCanvasOfflineReward;
        private readonly List<ViewGood> _viewGoods = new();
        private List<GoodItem> _rewardGoodItems = new();

        public ControllerCanvasOfflineReward(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasOfflineReward>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.GetRewardButton.onClick.AddListener(() => GetReward());
            View.Get2XRewardButton.onClick.AddListener(() =>
            {
                GoogleMobileAdsManager.Instance.ShowRewardedAd(() => GetReward(true));
            });
            
            Open();

            var originVector = View.DoubleTMP.localPosition;
            View.DoubleTMP
                .DOLocalJump(originVector, 10, 1, 0.5f)
                .SetLoops(int.MaxValue)
                .SetUpdate(true);
        }

        private void UpdateView(TimeSpan timeSpan)
        {
            var totalTime = DataController.Instance.offlineReward.GetTotalTime(GoodType.Gold);
            var hours = Mathf.Min(totalTime, (int)timeSpan.TotalHours);
            var minutes = hours >= totalTime
                ? 0
                : (int)timeSpan.TotalMinutes - (int)timeSpan.TotalHours * 60;

            var cumulativeTimeText = LocalizeManager.GetText(LocalizedTextType.OfflineRewardTime, hours, minutes);
            var desc = LocalizeManager.GetText(LocalizedTextType.OfflineRewardDescription, totalTime);

            View.SetDescriptionText(desc);
            View.ViewSlotTime.SetTimeText(cumulativeTimeText);

            var rewardViewGoods = _viewGoods.GetViewSlots(ViewSlotRewardName, View.SlotParent, _rewardGoodItems.Count);

            var i = 0;
            foreach (var goodItem in _rewardGoodItems)
            {
                var slot = rewardViewGoods[i];
                slot
                    .SetInit(goodItem.GoodType)
                    .SetValue(goodItem.Value)
                    .SetActive(true);
                ++i;
            }
        }

        public sealed override async void Open()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
            
            var timeSpan = DataController.Instance.offlineReward.GetOfflineTimeSpan();
            if (timeSpan.TotalMinutes < 1) return;
            
            _rewardGoodItems = DataController.Instance.offlineReward.GetRewardGoodItems();
            
            UpdateView(timeSpan);
             ViewCanvas.Open();
        }
        
        private void GetReward(bool isDouble = false)
        {
            foreach (var goodItem in _rewardGoodItems)
            {
                DataController.Instance.good.Earn(goodItem.GoodType, goodItem.Value * (isDouble ? 1 : 2));
            }
            Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(_rewardGoodItems, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
            DataController.Instance.LocalSave();
            Close();
        }
    }
}