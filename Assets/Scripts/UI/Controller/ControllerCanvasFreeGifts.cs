using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasFreeGifts : ControllerCanvas
    {
        private ViewCanvasFreeGifts View => ViewCanvas as ViewCanvasFreeGifts;
        
        private Sequence _openSequence;
        private Sequence _closeSequence;
        private List<ViewSlotProduct> _viewSlotProducts; 
        
        public ControllerCanvasFreeGifts(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasFreeGifts>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            
            _viewSlotProducts = new List<ViewSlotProduct>();
            var i = 0;
            foreach (var bData in DataController.Instance.freeGift.BDatas)
            {
                var slotParent = View.Slots[(int)bData.freeGiftType];
                var index = i;
                var slot = Object.Instantiate(View.ViewSlotProduct, slotParent.transform);

                slot.ViewGood.SetInit(GoodType.ShowAds);
                slot.Button.OnClick.AddListener(() => TryPurchase(index));
                
                _viewSlotProducts.Add(slot);
                UpdateSlot(index);
                ++i;
            }
            ServerTime.onBindNextDay += OnNextDay;

            TimeTask().Forget();
        }

        private void TryPurchase(int index)
        {
            if (DataController.Instance.freeGift.GetCurrPurchasedCount(index) <= 0) return;
            GoogleMobileAdsManager.Instance.ShowRewardedAd(() => GetReward(index));
        }

        private void GetReward(int index)
        {
            var rewardGoodType = DataController.Instance.freeGift.GetRewardType(index);
            var param = DataController.Instance.freeGift.GetParam(index);
            var rewardValues = DataController.Instance.freeGift.BDatas[index].rewardValues;
            
            var viewGood = Get<ControllerCanvasShop>().ViewGoods.FirstOrDefault(shopViewGood => rewardGoodType == shopViewGood.GoodType);
            if (!viewGood) viewGood = Get<ControllerCanvasShop>().ViewGoods[0];
            
            DataController.Instance.good.Earn(rewardGoodType, rewardValues, param);
            GoodsEffectManager.Instance.ShowEffect(rewardGoodType, Vector2.zero, viewGood, Mathf.Min((int)rewardValues, 10));
            DataController.Instance.freeGift.DiscountCurrPurchased(index);
            UpdateSlot(index);  
            
            DataController.Instance.LocalSave();
        }

        private void AsyncLayout()
        {
            const int minValue = 300;
            const float ratioValue = 0.76f;
            foreach (var slot in View.Slots)
            {
                if (slot.transform.parent.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    var width = Mathf.Min(minValue * ratioValue, (rectTransform.rect.width - 60) * 0.25f);
                    var height = Mathf.Min(minValue, rectTransform.rect.height - 90);

                    if (height * ratioValue > width)
                    {
                        height = width * (1 + (1 - ratioValue));
                    }
                    else
                    {
                        width = height * ratioValue;
                    }
                    foreach (Transform componentsInChild in slot.transform)
                    {
                        if(componentsInChild.TryGetComponent<RectTransform>(out var rect))
                        {
                            rect.sizeDelta = new Vector2(width, height);
                        }
                    }
                }
                slot.SetLayoutHorizontal();
                slot.SetLayoutVertical();
            }
        }

        private async UniTaskVoid TimeTask()
        {
            while (!Cts.IsCancellationRequested)
            {
                View.ViewSlotTime.SetTimeText(Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextDay));  
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private void UpdateSlot(int index)
        {
            if (_viewSlotProducts.Count <= index) return;

            var goodType = DataController.Instance.freeGift.GetRewardType(index);
            var rewardValue = DataController.Instance.freeGift.GetRewardValue(index);
            var param = DataController.Instance.freeGift.GetParam(index);
            var currCount = DataController.Instance.freeGift.GetCurrPurchasedCount(index);
            var currAndMaxPurchasedCountText =
                LocalizeManager.GetText(LocalizedTextType.ShowAdsCountForFreeReward,
                    currCount, DataController.Instance.freeGift.GetMaxPurchaseCount(index));

            _viewSlotProducts[index]
                .SetIcon(DataController.Instance.good.GetImage(goodType))
                .SetRewardText($"{rewardValue.ToGoodString(goodType, param, true)}")
                .SetPurchaseCountText(currAndMaxPurchasedCountText)
                .SetActive(true);
            
            _viewSlotProducts[index].Reddot.ShowReddot(currCount > 0);
        }

        public override void Open()
        {
            base.Open();
            AsyncLayout();
        }

        private void UpdateAllSlot()
        {
            for (var i = 0; i < _viewSlotProducts.Count; ++i)
            {
                UpdateSlot(i);
            }
        }

        private void OnNextDay()
        {
            DataController.Instance.freeGift.OnNextDay();
            UpdateAllSlot();
        }
    }
}