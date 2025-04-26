using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasVip : ControllerCanvas
    {
        private ViewCanvasVip View => ViewCanvas as ViewCanvasVip;

        public ControllerCanvasVip(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasVip>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            
            View.BoxCap.rectTransform.DOLocalMoveY(10f, 0.7f)
                .SetUpdate(true)
                .SetLoops(int.MaxValue, LoopType.Yoyo);
            View.GetRewardButton.onClick.AddListener(() =>
            {
                GetVipReward();
                DataController.Instance.vip.isGetReward = true;
                UpdateGetRewardButton();
                UpdateBox();
                DataController.Instance.LocalSave();
            });
            View.GetAdsRewardButton.onClick.AddListener(() =>
            {
                GoogleMobileAdsManager.Instance.ShowRewardedAd(() =>
                {
                    GetVipReward();
                    DataController.Instance.vip.isGetAdsReward = true;
                    UpdateGetRewardButton();
                    UpdateBox();
                    DataController.Instance.LocalSave();
                });
            });
            
            View.CheckNextVipButton.onClick.AddListener(() =>
            {
                Close();
                var canvasShop = Get<ControllerCanvasShop>();
                Get<ControllerCanvasBottomMenu>().bottomButtonGroup.Select(canvasShop.MenuIndex);
                canvasShop.ScrollToVipTicket().Forget();
            });

            DataController.Instance.shop.OnBindVipPurchased += () =>
            {
                DataController.Instance.vip.isGetReward = false;
                DataController.Instance.vip.isGetAdsReward = false;
                DataController.Instance.LocalSave();
                UpdateView();
            };

            ServerTime.onBindNextDay += UpdateView;
            
            UpdateView();
            TimeTask().Forget();
        }

        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            while (!Cts.IsCancellationRequested)
            {
                View.SetTimeText(Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextDay));
                UpdateGetRewardButton();
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private void GetVipReward()
        {
            var vipLevel = (int)DataController.Instance.good.GetValue(GoodType.VIP);
            var count = DataController.Instance.vip.GetVipRewardCount(vipLevel);

            var goodItems = new List<GoodItem>
            {
                new(GoodType.SummonElementalTicket, count),
                new(GoodType.SummonRuneTicket, count),
            };

            foreach (var goodItem in goodItems)
            {
                DataController.Instance.good.Earn(goodItem.GoodType, goodItem.Value);
            }

            Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(goodItems, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
        }
        
        private void UpdateView()
        {
            var vipLevel = (int)DataController.Instance.good.GetValue(GoodType.VIP);
            
            View
                .SetBenefitTitleText(LocalizeManager.GetText(LocalizedTextType.VIPBenefit, vipLevel))
                .SetRewardTitleText(LocalizeManager.GetText(LocalizedTextType.VIPRewardBox, vipLevel))
                .SetCheckNextVipText(LocalizeManager.GetText(LocalizedTextType.VIPPreviewBenefit, vipLevel + 1));
            
            if(vipLevel > 3) View.CheckNextVipButton.gameObject.SetActive(false);
            
            UpdateBenefit(vipLevel);
            UpdateGetRewardButton();
            UpdateBox();
        }

        private void UpdateBenefit(int vipLevel)
        {
            View.ViewSlotVip.SetActive(true);
            View.SetActiveEmptyText(false);
            
            var daily = LocalizeManager.GetText(LocalizedTextType.Daily);
            var elemental = LocalizeManager.GetText(LocalizedTextType.Shop_SummonElemental);
            var rune = LocalizeManager.GetText(LocalizedTextType.Shop_SummonRune);
                
            var count = DataController.Instance.vip.GetVipRewardCount(vipLevel);
                
            View.ViewSlotVip.SetReward(0, DataController.Instance.good.GetImage(GoodType.SummonElementalTicket), 
                $"{daily} {elemental} x{count}");
            View.ViewSlotVip.SetReward(1, DataController.Instance.good.GetImage(GoodType.SummonRuneTicket), 
                $"{daily} {rune} x{count}");

            if (vipLevel >= 1)
            {
                var productType = vipLevel switch
                {
                    < 2 => ProductType.net_themessage_etd_vip0,
                    < 3 => ProductType.net_themessage_etd_vip1,
                    < 4 => ProductType.net_themessage_etd_vip2_renew,
                    < 5 => ProductType.net_themessage_etd_vip3,
                    _ => ProductType.net_themessage_etd_vip0
                };

                var rewardGoodTypes = DataController.Instance.shop.GetRewardGoodTypes(productType);
                var rewardValues = DataController.Instance.shop.GetRewardValues(productType);

                var index = 2;
                for (var i = 0; i < rewardGoodTypes.Length; ++i)
                {
                    if (rewardGoodTypes[i] == GoodType.VIP) continue;
                    var sprite = DataController.Instance.good.GetImage(rewardGoodTypes[i]);
                    var desc = LocalizeManager.GetText(rewardGoodTypes[i], (float)rewardValues[i]);
                    View.ViewSlotVip.SetReward(index, sprite, desc);
                    index++;
                }
            }
        }

        private void UpdateBox()
        {
            var isGetRewards = DataController.Instance.vip.IsGetRewards();
            var sprite = View.GetBoxSprite(isGetRewards);
            
            View.SetBoxSprite(sprite);
            View.BoxCap.enabled = !isGetRewards;
        }

        private void UpdateGetRewardButton()
        {
            View.GetRewardButton.gameObject.SetActive(!DataController.Instance.vip.isGetReward);
            View.GetAdsRewardButton.gameObject.SetActive(!DataController.Instance.vip.isGetAdsReward);
            
            View.Reddot.ShowReddot(!DataController.Instance.vip.IsGetRewards());
        }
    }
}