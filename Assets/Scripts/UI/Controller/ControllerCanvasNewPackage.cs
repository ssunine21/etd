using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasNewPackage : ControllerCanvas
    {
        private const string ViewSlotGoodName = "ViewSlotGoodSquare";

        private ViewCanvasNewPackage View => ViewCanvas as ViewCanvasNewPackage;
        private List<ViewGood> _rewardViewGoods = new();
        private List<ViewPositionMark> _marks;
        private List<ProductType> _productTypes;
        private ProductType _currProductType;

        public ControllerCanvasNewPackage(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasNewPackage>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            _productTypes = new List<ProductType>();
            _marks = new List<ViewPositionMark>();
            
            InitTask().Forget();
        }

        private async UniTaskVoid InitTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            foreach (var enableNewPackageProductType in DataController.Instance.shop.GetEnableNewPackageProductTypes())
            {
                _productTypes.Add(enableNewPackageProductType);
            }
            
            TimeTask().Forget();
            
            View.PurchaseButton.onClick.AddListener(TryPurchase);
            View.NextButton.onClick.AddListener(() =>
            {
                if (TryGetCurrIndex(_currProductType, out var index))
                    UpdateView(_productTypes[Mathf.Clamp(index + 1, 0, _productTypes.Count - 1)]);
            });
            View.PrevButton.onClick.AddListener(() =>
            {
                if (TryGetCurrIndex(_currProductType, out var index))
                    UpdateView(_productTypes[Mathf.Clamp(index - 1, 0, _productTypes.Count - 1)]);
            });
        }

        public ControllerCanvasNewPackage Add(ProductType type)
        {
            _productTypes ??= new List<ProductType>();
            _productTypes.Add(type);
            return this;
        }

        public ControllerCanvasNewPackage Remove(ProductType type)
        {
            if (_productTypes.Contains(type))
                _productTypes.Remove(type);
            return this;
        }
        
        private void TryPurchase()
        {
            if (DataController.Instance.shop.GetCurrPurchasedCount(_currProductType) == 0) return;
            var productId = DataController.Instance.shop.GetProductId(_currProductType);
            
            if (!string.IsNullOrEmpty(productId))
            {
                IAPManager.Instance.Purchase(productId, (isSuccess) =>
                {
                    if (isSuccess)
                    {
                        GetReward();
                        Close();
                        Remove(_currProductType);
                        
                        DataController.Instance.SaveBackendData();
                    }
                });
            }
        }
        
        private void GetReward()
        {
            var rewardGoodItems = DataController.Instance.shop.GetRewardGoodItems(_currProductType); 
            
            DataController.Instance.shop.DiscountCurrPurchased(_currProductType);
            DataController.Instance.shop.onBindPurchased?.Invoke(_currProductType);
            DataController.Instance.good.EarnReward(rewardGoodItems);

            Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(rewardGoodItems, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
        }

        public ControllerCanvasNewPackage ShowView(ProductType type)
        {
            base.Open();
            UpdateView(type);
            return this;
        }
        
        public ControllerCanvasNewPackage ShowFirstView()
        {
            if (_productTypes.Count <= 0) return this;
            UpdateView(_productTypes[0]);
            return this;
        }

        public override void Open()
        {
            base.Open();
            ShowFirstView();
        }

        private async void UpdateView(ProductType type)
        {
            foreach (var rewardViewGood in _rewardViewGoods)
                rewardViewGood.SetActive(false);
            
            _currProductType = type;
            if (Enum.TryParse(type.ToString(), out LocalizedTextType result))
                View.ViewSlotProduct.SetTitle(LocalizeManager.GetText(result));

            var packageValue = DataController.Instance.shop.GetPackageValue(type);
            var productId = DataController.Instance.shop.GetProductId(type);
            var price = IAPManager.Instance.GetPrice(productId);
            var multiplePrice = IAPManager.Instance.GetMultiplePriceString(productId, packageValue);
            
            View.ViewSlotProduct
                .SetPackageValue(packageValue)
                .SetPurchaseCountText(LocalizeManager.GetText(LocalizedTextType.PurchaseCount, DataController.Instance.shop.GetMaxPurchaseCount(type)))
                .SetInAppPriceText(price)
                .SetMultiplePriceText(multiplePrice);

            var disableTime = DataController.Instance.shop.GetNewPackageDisableTimeToString(type);

            var timeSpan = ServerTime.RemainingTimeToTimeSpan(disableTime);
            View.SetRemainingTime(Utility.GetTimeStringToFromTotalSecond(timeSpan));

            var rewardGoodItems = DataController.Instance.shop.GetRewardGoodItems(type);
            var viewGoods = _rewardViewGoods.GetViewSlots(ViewSlotGoodName, View.ViewSlotProduct.RewardScrollRect.content, rewardGoodItems.Count);
            var i = 0;
            foreach (var rewardGoodItem in rewardGoodItems)
            {
                var goodTooltip = viewGoods[i];
                
                if (rewardGoodItem.GoodType == GoodType.SummonElemental)
                    goodTooltip.SetGrade(rewardGoodItem.GoodType, rewardGoodItem.Param0);

                goodTooltip
                    .SetInit(rewardGoodItem.GoodType, rewardGoodItem.Param0)
                    .SetValue(rewardGoodItem.Value.ToGoodString(rewardGoodItem.GoodType,0, true))
                    .SetActive(true);
                goodTooltip.transform.localScale = new Vector3(1, 0, 1);
                goodTooltip.transform
                    .DOScaleY(1, 0.25f)
                    .SetUpdate(true);

                await UniTask.Delay(150, true, PlayerLoopTiming.Update, Cts.Token);
                
                ++i;
            }

            if (TryGetCurrIndex(type, out var index))
            {
                SetMark(index);
                SetArrow(index);
            }
        }

        private bool TryGetCurrIndex(ProductType type, out int index)
        {
            for (var i = 0; i < _productTypes.Count; ++i)
            {
                if (_productTypes[i] == type)
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }

        private void SetMark(int index)
        {
            for (var i = _marks.Count; i < _productTypes.Count; ++i)
            {
                _marks.Add(Object.Instantiate(View.MarkPrefabs, View.MarkPrefabs.transform.parent));
            }

            for (var i = 0; i < _marks.Count; ++i)
            {
                _marks[i]
                    .On(i == index)
                    .SetActive(i < _productTypes.Count);
            }
        }

        private void SetArrow(int index)
        {
            View
                .SetEnablePrevButton(index > 0)
                .SetEnableNextButton(index < _productTypes.Count - 1);
        }
        
        private async UniTaskVoid TimeTask()
        {
            var remainingTime = "00:00:00";
            while (!Cts.IsCancellationRequested)
            {
                if(_productTypes?.Count > 0)
                {
                    var disableTime = DataController.Instance.shop.GetNewPackageDisableTimeToString(_currProductType);
                    if (ServerTime.IsRemainingTimeUntilDisable(disableTime))
                    {
                        var timeSpan = ServerTime.RemainingTimeToTimeSpan(disableTime);
                        remainingTime = Utility.GetTimeStringToFromTotalSecond(timeSpan);
                    }
                    else
                    {
                        Remove(_currProductType);
                    }

                    SetTimeInMainMenuPackageButton();
                }
                else
                {
                    DataController.Instance.shop.onBindNewPackageTime?.Invoke(string.Empty);
                }
                View.SetRemainingTime(remainingTime);
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private void SetTimeInMainMenuPackageButton()
        {
            var ticks = new TimeSpan(9999, 0, 0, 0);
            foreach (var disableTime in _productTypes.Select(productType => DataController.Instance.shop.GetNewPackageDisableTimeToString(productType)))
            {
                var remainTime = ServerTime.RemainingTimeToTimeSpan(disableTime);
                if (remainTime.TotalSeconds > 0)
                    ticks = ticks.TotalSeconds > remainTime.TotalSeconds ? remainTime : ticks;
            }

            var dateTimeToString = Utility.GetTimeStringToFromTotalSecond(ticks);
            DataController.Instance.shop.onBindNewPackageTime?.Invoke(dateTimeToString);
        }
    }
}