using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{

    public class ControllerProductSlotStepup
    {
        private readonly ViewSlotProductStepup _view;       
        private readonly CancellationTokenSource _cts;
        
        private readonly ProductType _productType;
        private readonly Dictionary<int, ProductType> _stepDic = new();
        private readonly List<ViewGood> _viewGoods = new();

        private ProductType _currProductType;
        
        public ControllerProductSlotStepup(ViewSlotProductStepup view, CancellationTokenSource cts)
        {
            _view = view;
            _cts = cts;
            _view.ViewGoodPrefab.SetActive(false);
            _productType = _view.ProductType;
            
            var ptStringBuilder = new StringBuilder(_productType.ToString());
            for (var i = 0; i < 10; ++i)
            {
                var length = ptStringBuilder.Length;
                if (length > 0)
                {
                    ptStringBuilder[length - 1] = (char)('0' + i);
                }

                if (Enum.TryParse(ptStringBuilder.ToString(), out ProductType result))
                {
                    _stepDic[i] = result;
                }
                else break;
            }
            
            _view.PurchaseButton.OnClick.AddListener(TryPurchase);
            _view.StepSlideButton.AddListener(UpdateView);
            _view.StepSlideButton.OnClick(0);
        }

        private void TryPurchase()
        {
            var productId = DataController.Instance.shop.GetProductId(_currProductType);
            if (string.IsNullOrEmpty(productId))
            {
                PurchaseCallback(true);
            }
            else
            {
                IAPManager.Instance.Purchase(productId, PurchaseCallback);
            }
        }

        private void PurchaseCallback(bool isSuccess)
        {
            var goodItems = DataController.Instance.shop.GetRewardGoodItems(_currProductType);
            DataController.Instance.good.EarnReward(goodItems);
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(goodItems, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
            
            DataController.Instance.shop.DiscountCurrPurchased(_currProductType);
            UpdateView(_view.StepSlideButton.SelectedIndex);
        }

        private void UpdateView(int index)
        {
            index = Mathf.Clamp(index, 0, _stepDic.Count - 1);
            if (_stepDic.TryGetValue(index, out var value))
            {
                var goodItems = DataController.Instance.shop.GetRewardGoodItems(value);
                var i = 0;
                foreach (var goodItem in goodItems)
                {
                    var viewGood = GetViewGoodSlot(i);
                    viewGood
                        .SetInit(goodItem.GoodType, goodItem.Param0)
                        .SetValue(goodItem.Value, goodItem.Param0)
                        .SetActive(true);
                    ++i;
                }

                for (; i < _viewGoods.Count; ++i)
                {
                    _viewGoods[i].SetActive(false);
                }

                var maxCount = DataController.Instance.shop.GetMaxPurchaseCount(value);
                var currCount = DataController.Instance.shop.GetCurrPurchasedCount(value);
                var productId = DataController.Instance.shop.GetProductId(value);
                var priceText = string.IsNullOrEmpty(productId)
                    ? LocalizeManager.GetText(LocalizedTextType.Free)
                    : IAPManager.Instance.GetPrice(value.ToString());

                _view
                    .SetPriceText(priceText)
                    .SetPurchaseCount($"{currCount}/{maxCount}");
                _view.PurchaseButton.Selected(currCount > 0 && Purchaseable(index));
                _currProductType = value;
            }
        }

        private bool Purchaseable(int index)
        {
            if (index > 0)
            {
                if (_stepDic.TryGetValue(index - 1, out var value))
                {
                    var currCount = DataController.Instance.shop.GetCurrPurchasedCount(value);
                    var maxCount = DataController.Instance.shop.GetMaxPurchaseCount(value);
                    return currCount < maxCount;
                }
            }

            return true;
        }

        private ViewGood GetViewGoodSlot(int index)
        {
            var count = _viewGoods.Count;
            for (var i = count; i <= index; ++i)
            {
                _viewGoods.Add(Object.Instantiate(_view.ViewGoodPrefab, _view.ViewGoodParent));
            }

            return _viewGoods[index];
        }
    }
}