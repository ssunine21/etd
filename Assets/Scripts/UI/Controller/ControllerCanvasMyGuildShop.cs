using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMyGuild
    {
        private const int ShopMenuIndex = 3;
        private readonly List<ViewSlotProduct> _viewSlotProducts = new();
        private readonly List<ViewSlotProduct> _protectionProducts = new();
        private void InitShop()
        {
            ShopTimeTask().Forget();
            View.ViewSlotProductPrefab.SetActive(false);
            
            InitShopGoods();
        }

        private async UniTaskVoid ShopTimeTask()
        {
            while (!Cts.IsCancellationRequested)
            {
                UpdateTime();
                UpdateProtectionProduct();
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private void InitShopGoods()
        {
            var i = 0;
            for (; i < DataController.Instance.guildReward.ShopProductCount; ++i)
            {
                var rewardGoodItem = DataController.Instance.guildReward.GetRewardGoodItem(GuildRewardType.Shop, i)?[0];
                var needGoodItem = DataController.Instance.guildReward.GetNeedGoodItem(GuildRewardType.Shop, i);
                var timeResetType = DataController.Instance.guildReward.GetTimeResetType(GuildRewardType.Shop, i);
                var maxCount = DataController.Instance.guildReward.MaxPurchaseCount(GuildRewardType.Shop, i);
                var currCount = DataController.Instance.guildReward.CurrPurchaseCount(GuildRewardType.Shop, i);
                if (rewardGoodItem == null) return;
            
                var productSlot = GetProductSlot(i, timeResetType);
                if(rewardGoodItem.GoodType == GoodType.Protection) _protectionProducts.Add(productSlot);

                if (needGoodItem != null)
                    productSlot.ViewGood.SetInit(needGoodItem.GoodType, needGoodItem.Param0).SetValue(needGoodItem.Value, needGoodItem.Param0);
            
                productSlot
                    .SetIcon(rewardGoodItem.GoodType)
                    .SetRewardText($"{rewardGoodItem.Value.ToGoodString(rewardGoodItem.GoodType, rewardGoodItem.Param0, true)}")
                    .SetActiveBookmark(timeResetType != TimeResetType.None)
                    .SetActive(true);

                var index = i;
                productSlot.Button.OnClick.AddListener(() =>
                {
                    if (currCount >= maxCount && maxCount > -1) return;
                    if(TryGetShopReward(rewardGoodItem, needGoodItem))
                    {
                        Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(rewardGoodItem, LocalizeManager.GetText(LocalizedTextType.Claimed));
                        DataController.Instance.guildReward.IncreaseCurrPurchaseCount(GuildRewardType.Shop, index);
                        UpdateShopGood(index);
                    }
                });
                
                UpdateShopGood(i);
            }

            for (; i < _viewSlotProducts.Count; ++i)
            {
                _viewSlotProducts[i].SetActive(false);
            }

            ServerTime.onBindNextWeek += OnNextWeek;
        }

        private void OnNextWeek()
        {
            for (var i = 0; i < DataController.Instance.guildReward.ShopProductCount; ++i)
            {
                DataController.Instance.guildReward.SetCurrPurchaseCount(GuildRewardType.Shop, i, 0);
            }
        }

        private void UpdateProtectionProduct()
        {
            foreach (var protectionProduct in _protectionProducts)
            {
                protectionProduct.SetTimeLockPanel(DataController.Instance.player.IsProtected());
                if (DataController.Instance.player.IsProtected())
                {
                    var timeSpan = DataController.Instance.player.GetProtectRemainTimeSpan();
                    var timeText = Utility.GetTimeStringToFromTotalSecond(timeSpan);
                    var text = $"<color=green>({LocalizeManager.GetText(LocalizedTextType.Protect)})</color>" +
                               $"\n{timeText}";

                    protectionProduct.SetRemainingTimeText(text);
                }
            }
        }

        private void UpdateShopGood(int index)
        {
            var dataGuildReward = DataController.Instance.guildReward;
            if (index >= dataGuildReward.ShopProductCount) return;
            
            var maxCount = DataController.Instance.guildReward.MaxPurchaseCount(GuildRewardType.Shop, index);
            var currCount = DataController.Instance.guildReward.CurrPurchaseCount(GuildRewardType.Shop, index);
            
            var productSlot = GetProductSlot(index);
            productSlot
                .SetBookmarkText($"{maxCount - currCount}/{maxCount}")
                .SetTimeLockPanel(maxCount > -1 && currCount >= maxCount)
                .SetRemainingTimeText("")
                .SetActive(true);
        }

        private bool TryGetShopReward(GoodItem rewardGoodItem, GoodItem needGoodItem)
        {
            if (DataController.Instance.good.TryConsume(needGoodItem.GoodType, needGoodItem.Value))
            {
                if (rewardGoodItem.GoodType == GoodType.Protection && DataController.Instance.player.IsProtected()) return false;
                
                DataController.Instance.good.EarnReward(rewardGoodItem.GoodType, rewardGoodItem.Value, rewardGoodItem.Param0);
                return true;
            }

            return false;
        }

        private ViewSlotProduct GetProductSlot(int index, TimeResetType timeResetType = TimeResetType.None)
        {
            var count = _viewSlotProducts.Count;
            for (var i = count; i <= index; ++i)
            {
                _viewSlotProducts.Add(Object.Instantiate(View.ViewSlotProductPrefab, View.ViewSlotProductParent[GetParerntIndex(timeResetType)]));
            }

            return _viewSlotProducts[index];
        }

        private int GetParerntIndex(TimeResetType timeResetType)
        {
            return timeResetType switch
            {
                TimeResetType.Weekly => 0,
                _ => 1
            };
        }

        private void UpdateTime()
        {
            var time = Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextWeek);
            View.SetWeeklyResetTime(time);
        }
    }
}