using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.CloudData;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerProductSlot
    {
        public ProductType ProductType => _productType;
        public ViewSlotProduct View => _view;
        
        private List<ViewGood> _rewardViewGoodTooltips;
        
        public ControllerProductSlot(ViewSlotProduct view, CancellationTokenSource cts)
        {
            _cts = cts;
            _view = view;
            _productType = _view.ProductType;
            _productId = DataController.Instance.shop.GetProductId(_productType);

            if (Enum.TryParse(_productId, out LocalizedTextType result))
                _view.SetTitle(LocalizeManager.GetText(result));
            
            _view.Button.OnClick.RemoveAllListeners();
            _view.Button.OnClick.AddListener(TryPurchase);
            _view.SetPackageValue(DataController.Instance.shop.GetPackageValue(_productType));

            DataController.Instance.shop.onBindPurchased += UpdateView;
            DataController.Instance.good.OnBindChangeGood += (goodType) =>
            {
                if (goodType is GoodType.SummonElementalTicket or GoodType.SummonRuneTicket)
                    UpdateView();
            };
            
            DataController.Instance.shop.onBindInitialized += UpdateView;

            SetRewardView();
            UpdateView();
            TimeAttackTask().Forget();
            IAPSetting().Forget();
        }
        
        private readonly ViewSlotProduct _view;       
        private readonly CancellationTokenSource _cts;
        
        private readonly ProductType _productType;
        private GoodType _goodType;
        private string _productId;
        private double _price;

        private async UniTaskVoid IAPSetting()
        {
            await UniTask.WaitUntil(() => IAPManager.Instance.IsInitialized, PlayerLoopTiming.Update, _cts.Token);
            
            var productId = DataController.Instance.shop.GetProductId(_productType);
            Utility.LogWithColor($"ProductId: {productId}", Color.magenta);
            _view.SetActiveViewGood(string.IsNullOrEmpty(productId));
            if (!string.IsNullOrEmpty(productId))
            {
                var price = IAPManager.Instance.GetPrice(productId);
                Utility.LogWithColor($"Price: {price}", Color.magenta);
                _view.SetInAppPriceText(price);
            }
        }
        
        private async UniTaskVoid TimeAttackTask()
        {
            while (DataController.Instance.shop.GetMaxPurchaseCount(_productType) > 0)
            {
                if(_view.IsActiveTimeLockPanel)
                {
                    var nextTimeSpan = DataController.Instance.shop.GetTimeResetType(_productType) switch
                    {
                        TimeResetType.Daily => ServerTime.RemainingTimeUntilNextDay,
                        TimeResetType.Weekly => ServerTime.RemainingTimeUntilNextWeek,
                        _ => TimeSpan.Zero
                    };
                    
                    _view.SetRemainingTimeText(Utility.GetTimeStringToFromTotalSecond(nextTimeSpan));
                }
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, _cts.Token);
            }
        }

        private void TryPurchase()
        {
            if (DataController.Instance.shop.GetCurrPurchasedCount(_productType) == 0) return;
            
            if (_productType.ToString().Contains("SummonRuneFor") && DataController.Instance.rune.IsMaxRune)
            {
                ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.IsRunePull);
                return;
            }
            
            var rewardGoodTypes = DataController.Instance.shop.GetRewardGoodTypes(_productType);
            foreach (var rewardGoodType in rewardGoodTypes)
            {
                if (rewardGoodType == GoodType.Protection)
                {
                    if (DataController.Instance.player.IsProtected())
                    {
                        ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.IsProtectedDesc);
                        return;
                    }
                }
            }
            
            
            if (_goodType == GoodType.ShowAds)
                GoogleMobileAdsManager.Instance.ShowRewardedAd(GetReward);
            else
            {
                if (!string.IsNullOrEmpty(_productId))
                {
                    IAPManager.Instance.Purchase(_productId, (isSuccess) =>
                    {
                        if (isSuccess)
                        {
                            GetReward();
                            DataController.Instance.SaveBackendData();
                        }
                    });
                }
                else if (DataController.Instance.good.TryConsume(_goodType, _price))
                    GetReward();
            }
        }

        private void GetReward()
        {
            var rewardGoodItems = DataController.Instance.shop.GetRewardGoodItems(_productType);
            
            DataController.Instance.shop.DiscountCurrPurchased(_productType);
            DataController.Instance.shop.onBindPurchased?.Invoke(_productType);
            DataController.Instance.good.EarnReward(rewardGoodItems);

            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(rewardGoodItems, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
        }

        private void SetRewardView()
        {
            if (DataController.Instance.shop.GetPackageValue(_productType) < 1) return;
            
            var rewardGoodTyeps = DataController.Instance.shop.GetRewardGoodTypes(_productType);
            var rewardValues = DataController.Instance.shop.GetRewardValues(_productType);
            var rewardParams = DataController.Instance.shop.GetRewardParam0(_productType);

            for (var i = 0; i < rewardGoodTyeps.Length; ++i)
            {
                var goodTooltip = GetViewGoodTooltip(i);
                goodTooltip
                    .SetInit(rewardGoodTyeps[i])
                    .SetValue(rewardValues[i].ToGoodString(rewardGoodTyeps[i], rewardParams[i], true));
            }
        }
        
        private void UpdateView()
        {
            var maxCount = DataController.Instance.shop.GetMaxPurchaseCount(_productType);
            var currCount = DataController.Instance.shop.GetCurrPurchasedCount(_productType);
            var timeResetType = DataController.Instance.shop.GetTimeResetType(_productType);
            
            string priceText;

            if (DataController.Instance.shop.TryGetAlternativeGoods(_productType, out var keyValuePair)
                && DataController.Instance.good.GetValue(keyValuePair.Key) >= keyValuePair.Value)
            {
                var playerGoodValue = DataController.Instance.good.GetValue(keyValuePair.Key);
                _goodType = keyValuePair.Key;
                _price = keyValuePair.Value;
                priceText = $"{playerGoodValue} / {keyValuePair.Value}";
            }
            else
            {
                var needGoodPair = DataController.Instance.shop.GetNeededGoods(_productType);
                _goodType = needGoodPair.Key;
                _price = needGoodPair.Value;
                priceText = _price.ToGoodString(_goodType);
            }
            
            _view.ViewGood
                .SetInit(_goodType)
                .SetValue(priceText);
            
            if(maxCount > 0)
            {
                var currAndMaxPurchasedCountText =
                    _goodType == GoodType.ShowAds
                        ? LocalizeManager.GetText(LocalizedTextType.ShowAdsCountForFreeReward, currCount, maxCount)
                        : LocalizeManager.GetText(LocalizedTextType.Shop_CurrPurchasedCount, currCount, maxCount);

                if (DataController.Instance.shop.GetPackageValue(_productType) > 0)
                    currAndMaxPurchasedCountText = LocalizeManager.GetText(LocalizedTextType.Purchaseable, currCount, maxCount);
                
                _view
                    .SetPurchaseCountText(currAndMaxPurchasedCountText)
                    .SetTimeLockPanel(currCount == 0)
                    .SetRemainingTimeText(Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextDay));

                if (currCount == 0 && timeResetType == TimeResetType.None)
                {
                    _view.SetActive(false);
                    var parent = _view.transform.parent;
                    var hasChildren = parent.GetComponentsInChildren<ViewSlotProduct>().Length > 0;
                    parent.parent.gameObject.SetActive(hasChildren);
                }
            }
            else
            {
                var rewardGoodTyeps = DataController.Instance.shop.GetRewardGoodTypes(_productType);
                var reward = DataController.Instance.shop.GetRewardValues(_productType);
                var rewardParams = DataController.Instance.shop.GetRewardParam0(_productType);
                if (reward.Length <= 1)
                {
                    var text = reward[0].ToGoodString(rewardGoodTyeps[0], rewardParams[0], true);
                    _view.SetRewardText(text);
                }
            }
        }

        private void UpdateView(ProductType type)
        {
            if (type == _productType) UpdateView();
        }

        private ViewGood GetViewGoodTooltip(int index)
        {
            _rewardViewGoodTooltips ??= new List<ViewGood>();
            if (_rewardViewGoodTooltips.Count <= index)
            {
                var tooltip = Object.Instantiate(_view.ViewGoodTooltipPrefab, _view.RewardScrollRect.content);
                _rewardViewGoodTooltips.Add(tooltip);
            }
            
            _rewardViewGoodTooltips[index].gameObject.SetActive(true);
            return _rewardViewGoodTooltips[index];
        }
    }
}