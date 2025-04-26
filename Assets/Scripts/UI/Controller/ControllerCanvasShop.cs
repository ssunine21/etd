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

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasShop : ControllerCanvas
    {
        public int MenuIndex => 4;
        public ViewGood[] ViewGoods => View.ViewGoods;

        private readonly List<ControllerProductSlot> _controllerProductSlots;
        private readonly List<ControllerProductSlotStepup> _controllerProductSlotStepups = new();

        private ViewCanvasShop View => ViewCanvas as ViewCanvasShop;
        private Sequence _openSequence;
        private Sequence _closeSequence;

        private bool _isAnimationing;

        public ControllerCanvasShop(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasShop>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            _controllerProductSlots = new List<ControllerProductSlot>();
            for (var i = 0; i < View.SummonElementalViews.Length; ++i)
            {
                _controllerProductSlots.Add(new ControllerProductSlot(View.SummonElementalViews[i], cts));
                _controllerProductSlots.Add(new ControllerProductSlot(View.SummonRuneViews[i], cts));
            }
            
            View.ElementalProbabilityButton.onClick.AddListener(() => ShowProbability(ProbabilityType.SummonElemental));
            View.RuneProbabilityButton.onClick.AddListener(() => ShowProbability(ProbabilityType.SummonRune));
            View.ProtectedDescButton.onClick.AddListener(() =>
            {
                Get<ControllerCanvasToastMessage>().SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.Protection),
                    LocalizeManager.GetText(LocalizedTextType.ProtectionDesc),
                    LocalizeManager.GetText(LocalizedTextType.Confirm))
                    .ShowToastMessage();
            });
            
            View.SlideButton.AddListener(index =>
            {
                for (var i = 0; i < View.Contents.Length; ++i)
                {
                    View.Contents[i].SetActive(i == index);
                }
            });

            UpdateVIPPanel().Forget();
            foreach (var packageView in View.PackageViews)
            {
                _controllerProductSlots.Add(new ControllerProductSlot(packageView, cts));
            }

            foreach (var viewSlotProductStepup in View.ViewSlotProductStepups)
            {
                _controllerProductSlotStepups.Add(new ControllerProductSlotStepup(viewSlotProductStepup, cts));
            }
            
            TimeTask().Forget();
        }

        private async UniTaskVoid TimeTask()
        {
            while (true)
            {
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
                if (DataController.Instance.player.IsProtected())
                {
                    var timeSpan = DataController.Instance.player.GetProtectRemainTimeSpan();
                    var timeText = Utility.GetTimeStringToFromTotalSecond(timeSpan);
                    var text = $"<color=green>({LocalizeManager.GetText(LocalizedTextType.Protect)})</color>" +
                               $"\n{timeText}";
                    View.SetActiveProtectionProductLock(true, text);
                }
                else
                {
                    View.SetActiveProtectionProductLock(false);
                }
            }
        }
        
        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.SummonElemental =>
                    _controllerProductSlots.First(slot => slot.ProductType == ProductType.SummonElementalForDias).View.gameObject,
                TutorialType.SummonRune =>
                    _controllerProductSlots.First(slot => slot.ProductType == ProductType.SummonRuneForDias).View.gameObject,
                _ => null
            };
        }

        public async UniTaskVoid ScrollToSummonRuneRect()
        {
            await UniTask.WaitUntil(() => View.isActiveAndEnabled && !_isAnimationing);
            View.SlideButton.OnClick(2);
            ScrollToTargetPosition(View.SummonRuneViews[0].GetComponent<RectTransform>());
        }

        public async UniTaskVoid ScrollToSummonElementalRect()
        {
            await UniTask.WaitUntil(() => View.isActiveAndEnabled && !_isAnimationing);
            View.SlideButton.OnClick(2);
            ScrollToTargetPosition(View.SummonElementalViews[0].GetComponent<RectTransform>());
        }

        public async UniTaskVoid ScrollToVipTicket()
        {
            await UniTask.WaitUntil(() => View.isActiveAndEnabled && !_isAnimationing);
            View.SlideButton.OnClick(0);
            ScrollToTargetPosition(View.ViewSlotVip.GetComponent<RectTransform>());
        }
        
        private async UniTaskVoid UpdateVIPPanel()
        {
            var vipLevel = (int)DataController.Instance.good.GetValue(GoodType.VIP);
            if (vipLevel >= 4)
            {
                View.ViewSlotVip.SetActive(false);
                return;
            }
            
            var productType = vipLevel switch
            {
                < 1 => ProductType.net_themessage_etd_vip0,
                < 2 => ProductType.net_themessage_etd_vip1,
                < 3 => ProductType.net_themessage_etd_vip2_renew,
                < 4 => ProductType.net_themessage_etd_vip3,
                _ => ProductType.net_themessage_etd_vip0
            };
            
            await UniTask.WaitUntil(() => IAPManager.Instance.IsInitialized);
            
            View.ViewSlotVip
                .SetTitle(LocalizeManager.GetText(GoodType.VIP, (float)(vipLevel + 1)))
                .SetPrice(IAPManager.Instance.GetPrice(productType.ToString()))
                .SetActive(true);
            
            View.ViewSlotVip.VIPPurchaseButton.onClick.RemoveAllListeners();
            View.ViewSlotVip.VIPPurchaseButton.onClick.AddListener(() =>
            {
                IAPManager.Instance.Purchase(productType.ToString(), (isSuccess) =>
                {
                    if (isSuccess)
                    {
                        var rewardGoodTypes = DataController.Instance.shop.GetRewardGoodTypes(productType);
                        var rewardValues = DataController.Instance.shop.GetRewardValues(productType);
                        for (var i = 0; i < rewardGoodTypes.Length; ++i)
                        {
                            DataController.Instance.good.SetValue(rewardGoodTypes[i], rewardValues[i]);
                        }
                        DataController.Instance.SaveBackendData();
                        DataController.Instance.buff.OnBindGameSpeed?.Invoke();
                        UpdateVIPPanel().Forget();
                        
                        DataController.Instance.shop.OnBindVipPurchased?.Invoke();
                        Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(new GoodItem(GoodType.VIP, 1), LocalizeManager.GetText(LocalizedTextType.Claimed));
                    }
                });
            });

            var rewardGoodTypes = DataController.Instance.shop.GetRewardGoodTypes(productType);
            var rewardValues = DataController.Instance.shop.GetRewardValues(productType);

            var daily = LocalizeManager.GetText(LocalizedTextType.Daily);
            var elemental = LocalizeManager.GetText(LocalizedTextType.Shop_SummonElemental);
            var rune = LocalizeManager.GetText(LocalizedTextType.Shop_SummonRune);
            var count = DataController.Instance.vip.GetVipRewardCount(vipLevel + 1);
            
            View.ViewSlotVip.SetReward(0, DataController.Instance.good.GetImage(GoodType.SummonElementalTicket), 
                $"{daily} {elemental} x{count}");
            View.ViewSlotVip.SetReward(1, DataController.Instance.good.GetImage(GoodType.SummonRuneTicket), 
                $"{daily} {rune} x{count}");

            var index = 2;
            for (var i = 0; i < rewardGoodTypes.Length; ++i)
            {
                if(rewardGoodTypes[i] == GoodType.VIP) continue;
                var sprite = DataController.Instance.good.GetImage(rewardGoodTypes[i]);
                var desc = LocalizeManager.GetText(rewardGoodTypes[i], (float)rewardValues[i]);
                View.ViewSlotVip.SetReward(index, sprite, desc);
                index++;
            }
        }
        
        private void ShowProbability(ProbabilityType probabilityType)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            DataController.Instance.probability.GetProbability(probabilityType,  probabilityValue=>
            {
                if (probabilityValue is { Count: > 0 })
                {
                    var dic = new Dictionary<string, float>
                    {
                        { "C", probabilityValue[0] }, { "B", probabilityValue[1] },
                        { "A", probabilityValue[2] }, { "S", probabilityValue[3] },
                        { "SS", probabilityValue[4] },
                    };
                    var controller = Get<ControllerCanvasProbability>();
                    controller.AddOptions(dic);
                    controller.Open();
                }
                Get<ControllerCanvasToastMessage>().CloseLoading();
            });
        }
        
        private void ScrollToTargetPosition(RectTransform target)
        {
            var contentRect = View.ScrollRect.content;
            var localPosition = contentRect.InverseTransformPoint(target.position);
            var normalizedPositionY = Mathf.Clamp01(1 - (Mathf.Abs(localPosition.y) / contentRect.rect.height));

            View.ScrollRect.verticalNormalizedPosition = normalizedPositionY;
        }
    }
}