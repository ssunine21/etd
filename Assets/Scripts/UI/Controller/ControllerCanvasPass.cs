using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasPass : ControllerCanvas
    {
        private readonly List<ViewSlotPass> _viewSlotPasses = new();
        private const string ViewSlotPassName = nameof(ViewSlotPass);

        private ViewCanvasPass View => ViewCanvas as ViewCanvasPass;
        private ButtonRadioGroup _buttonRadioGroup = new();
        private bool _isPElementalSelectedView;
        
        public ControllerCanvasPass(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasPass>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.GuideViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.ElementalSelectedViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            
            View.OpenGuideButton.onClick.AddListener(() => View.GuideViewCanvasPopup.Open());
            
            View.GetRewardSelectedElementalButton.Button.onClick.AddListener(() =>
            {
                if(View.GetRewardSelectedElementalButton.IsSelected)
                {
                    var battlePassType = _isPElementalSelectedView ? BattlePassType.Premium : BattlePassType.Normal;
                    TryGetReward(0, battlePassType);
                    View.ElementalSelectedViewCanvasPopup.Close();
                }
            });
            
            View.PremiumPurchaseActiveButton.Button.onClick.AddListener(() =>
            {
                IAPManager.Instance.Purchase(IAPType.net_themessage_etc_premiumpass.ToString(), (isSuccess) =>
                {
                    if (!isSuccess) return;
                    View.PremiumPurchaseActiveButton.Selected(false);
                    DataController.Instance.pass.SetPremium(true);
                    DataController.Instance.SaveBackendData();
                    UpdateExp();
                    UpdateButton();
                    UpdateViewSlots();
                });
            });
            
            View.MasterurPchaseActiveButton.Button.onClick.AddListener(() =>
            {
                IAPManager.Instance.Purchase(IAPType.net_themessage_etc_masterpass.ToString(), (isSuccess) =>
                {
                    if (!isSuccess) return;
                    View.MasterurPchaseActiveButton.Selected(false);
                    DataController.Instance.pass.SetMasterLevel();
                    DataController.Instance.pass.SetPremium(true);
                    DataController.Instance.SaveBackendData();
                    UpdateExp();
                    UpdateButton();
                    UpdateViewSlots();
                });
            });
            
            View.SetGuideDescription(
                LocalizeManager.GetText(LocalizedTextType.Pass_Description0,
                    DataController.Instance.pass.GetIncreaseExpAmount(TimeResetType.Daily),
                    DataController.Instance.pass.GetIncreaseExpAmount(TimeResetType.Weekly),
                    DataController.Instance.pass.GetIncreaseExpAmount(TimeResetType.Repeat)));

            foreach (var slotUI in View.ViewSlotUis)
            {
                _buttonRadioGroup.AddListener(slotUI.ActiveButton);
            }

            _buttonRadioGroup.onBindSelected += index =>
            {
                ChangeSelectElemental();
                UpdateViewSlot(0);
            };

            DataController.Instance.mission.onBindGetRewarded += (resetType, missionType, isAds) =>
            {
                DataController.Instance.pass.IncreaseExp(resetType);
            };
            DataController.Instance.pass.OnBindChangeExp += () =>
            {
                var level = DataController.Instance.pass.GetLevel();
                UpdateExp();
                UpdateViewSlot(level - 1);
                UpdateViewSlot(level);
            };
            
            UpdateExp();
            InitViewSlots();
            
            _viewSlotPasses[0].ElementalSelectedButtons[0].onClick.AddListener(() => SetElementalSelectedView(BattlePassType.Normal));
            _viewSlotPasses[0].ElementalSelectedButtons[1].onClick.AddListener(() => SetElementalSelectedView(BattlePassType.Premium));
            
            TimeTask().Forget();
            IAPSetting().Forget();
        }
        
        public override void Open()
        {
            base.Open();
            
            AutoScroll();
            UpdateButton();
        }
        
        private void ShowElementalSelectedView(ElementalType currElementalType, GradeType gradeType)
        {
            View.ElementalSelectedViewCanvasPopup.Open();
            
            var i = 0;
            foreach (var viewSlotUI in View.ViewSlotUis)
            {
                viewSlotUI
                    .SetUnitSprite(DataController.Instance.elemental.GetImage((ElementalType)i, gradeType))
                    .SetGradeText(gradeType);
                ++i;

                if ((ElementalType)i == currElementalType)
                {
                    _buttonRadioGroup.Select(i);
                }
            }
        }

        private void UpdateExp()
        {
            var level = DataController.Instance.pass.GetLevel();
            var currExp = DataController.Instance.pass.GetRemainExp();
            var maxExp = DataController.Instance.pass.GetMaxExp(level - 1);
            var fillAmount = (float)currExp / maxExp;
            
            View
                .SetLevel(level)
                .SetExp($"{currExp} / {maxExp}")
                .SetExpFillAmount(fillAmount);
        }

        private void UpdateButton()
        {
            var isPremium = DataController.Instance.pass.IsPremium();
            var isMaster = DataController.Instance.pass.IsPremium() && DataController.Instance.pass.IsMaxLevel;
            
            View.PremiumPurchaseActiveButton.Selected(!isPremium);
            View.PremiumPurchaseActiveButton.Button.enabled = !isPremium;
            View.MasterurPchaseActiveButton.Selected(!isMaster);
            View.MasterurPchaseActiveButton.Button.enabled = !isMaster;
        }

        private void InitViewSlots()
        {
            var viewSlots = _viewSlotPasses.GetViewSlots(ViewSlotPassName, View.SlotParent, DataController.Instance.pass.BLength);
            for (var i = 0; i < _viewSlotPasses.Count; ++i)
            {
                var index = i;
                if (index == 0)
                {
                    viewSlots[i].NViewGood.AddListener(() => SetElementalSelectedView(BattlePassType.Normal));
                    viewSlots[i].PViewGood.AddListener(() => SetElementalSelectedView(BattlePassType.Premium));
                }
                else
                {
                    viewSlots[i].NViewGood.AddListener(() => TryGetReward(index, BattlePassType.Normal));
                    viewSlots[i].PViewGood.AddListener(() => TryGetReward(index, BattlePassType.Premium));
                }
                UpdateViewSlot(i);
            }
        }

        private void TryGetReward(int index, BattlePassType battlePassType)
        {
            if (!DataController.Instance.pass.CanReceiveReward(index, battlePassType)) return;

            var rewardGoodType = DataController.Instance.pass.GetRewardGoodType(index, battlePassType);
            var rewardValue = DataController.Instance.pass.GetRewardValue(index, battlePassType);
            var rewardParam0 = DataController.Instance.pass.GetRewardParam0(index, battlePassType);
            
            DataController.Instance.pass.HasReceived(index, battlePassType);
            DataController.Instance.good.EarnReward(rewardGoodType, rewardValue, rewardParam0);
            GoodsEffectManager.Instance.ShowEffect(rewardGoodType, Vector2.zero, null, Mathf.Min((int)rewardValue, 10));

            UpdateViewSlot(index);
            AutoScroll();
        }

        private void ChangeSelectElemental()
        {
            var battlePassType = _isPElementalSelectedView ? BattlePassType.Premium : BattlePassType.Normal;
            var elementalType = (ElementalType)_buttonRadioGroup.SelectedIndex;
            var grade = DataController.Instance.elemental.GetBData(
                DataController.Instance.pass.GetSelectedElementalIndex(battlePassType)).grade;

            var elementalIndex = DataController.Instance.elemental.GetBData(elementalType, grade).index;
            DataController.Instance.pass.SetSelectedElementalIndex(elementalIndex, battlePassType);
        }
        
        private void UpdateViewSlots()
        {
            for (var i = 0; i < _viewSlotPasses.Count; ++i)
            {
                UpdateViewSlot(i);
            }
        }

        private void UpdateViewSlot(int index)
        {
            if (_viewSlotPasses.Count <= index) return;
            var bData = DataController.Instance.pass;
            var slot = _viewSlotPasses[index];

            var nGoodType = bData.GetRewardGoodType(index, BattlePassType.Normal);
            var nValue = bData.GetRewardValue(index, BattlePassType.Normal);
            var nParam0 = bData.GetRewardParam0(index, BattlePassType.Normal);
            
            var pGoodType = bData.GetRewardGoodType(index, BattlePassType.Premium);
            var pValue = bData.GetRewardValue(index, BattlePassType.Premium);
            var pParam0 = bData.GetRewardParam0(index, BattlePassType.Premium);

            if (index == 0)
            {
                nParam0 = DataController.Instance.pass.GetSelectedElementalIndex(BattlePassType.Normal);
                pParam0 = DataController.Instance.pass.GetSelectedElementalIndex(BattlePassType.Premium);
            }

            slot.NViewGood
                .SetInit(nGoodType, nParam0)
                .SetValue(nValue, nParam0)
                .SetGrade(nGoodType, nParam0);
            slot.PViewGood
                .SetInit(pGoodType, pParam0)
                .SetValue(pValue, pParam0)
                .SetGrade(pGoodType, pParam0);

            var level = bData.GetLevel();
            var fillAmount = index < level ? 1
                : index > level ? 0
                : (float)bData.GetRemainExp() / bData.GetMaxExp(index);

            var slotLevel = index + 1;
            slot
                .SetLevel(slotLevel)
                .SetFillAmount(fillAmount)
                .SetActiveLockPanel(level <= index ,level <= index || !bData.IsPremium())
                .SetActiveCheckPanel(!bData.HasReceivedReward(index, BattlePassType.Normal), BattlePassType.Normal)
                .SetActiveCheckPanel(!bData.HasReceivedReward(index, BattlePassType.Premium), BattlePassType.Premium)
                .SetActiveElementalSelectedButton(index == 0)
                .ShowReddot(bData.CanReceiveReward(index, BattlePassType.Normal), bData.CanReceiveReward(index, BattlePassType.Premium))
                .SetActive(true);
        }
        
        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            
            var remainTime = ServerTime.IsoStringToDateTime(DataController.Instance.pass.GetRemainTimeToString()); 
            while (true)
            {
                if (ServerTime.IsRemainingTimeUntilDisable(remainTime))
                {
                    var timeSpan = ServerTime.RemainingTimeToTimeSpan(remainTime);
                    var remainTimeToText = Utility.GetTimeStringToFromTotalSecond(timeSpan);
                    View.ViewSlotTime.SetTimeText(remainTimeToText);
                }
                else
                {
                    ResetData();
                    remainTime = ServerTime.IsoStringToDateTime(DataController.Instance.pass.GetRemainTimeToString());
                }
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            }
        }
        
        private async UniTaskVoid IAPSetting()
        {
            await UniTask.WaitUntil(() => IAPManager.Instance.IsInitialized, PlayerLoopTiming.Update, Cts.Token);
            
            var productId = DataController.Instance.shop.GetProductId(ProductType.net_themessage_etc_premiumpass);
            var masterProductId = DataController.Instance.shop.GetProductId(ProductType.net_themessage_etc_masterpass);
            
            var price = IAPManager.Instance.GetPrice(productId);
            var masterPrice = IAPManager.Instance.GetPrice(masterProductId);

            View.PremiumPurchaseActiveButton
                .SetActiveButtonText(price)
                .SetInactiveButtonText(price);
            View.MasterurPchaseActiveButton
                .SetActiveButtonText(masterPrice)
                .SetInactiveButtonText(masterPrice);
        }

        private void ResetData()
        {
            DataController.Instance.pass.ResetData();
            
            UpdateExp();
            UpdateButton();
            UpdateViewSlots();
        }

        private void SetElementalSelectedView(BattlePassType battlePassType)
        {
            _isPElementalSelectedView = battlePassType != BattlePassType.Normal;
            var elemental = DataController.Instance.elemental.GetBData(DataController.Instance.pass.GetSelectedElementalIndex(battlePassType));
            ShowElementalSelectedView(elemental.elementalType, elemental.grade);

            View.GetRewardSelectedElementalButton.Selected(
                DataController.Instance.pass.CanReceiveReward(0, battlePassType));
        }

        private void AutoScroll()
        {
            for (var i = 0; i < _viewSlotPasses.Count; ++i)
            {
                if (DataController.Instance.pass.CanReceiveReward(i, BattlePassType.Normal) ||
                    DataController.Instance.pass.CanReceiveReward(i, BattlePassType.Premium))
                {
                    Utility.ScrollKill();
                    Utility.ScrollToTarget(View.ScrollRect, i, 5f).Forget();
                    break;
                }
            }
        }
    }
}