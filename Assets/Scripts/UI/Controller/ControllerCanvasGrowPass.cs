using System;
using System.Collections.Generic;
using System.Threading;
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
    public class ControllerCanvasGrowPass : ControllerCanvas
    {
        private ViewCanvasGrowPass View => ViewCanvas as ViewCanvasGrowPass;
        private readonly ButtonRadioGroup _passListGroup;
        private readonly ButtonRadioGroup _levelGroup;
        private readonly List<ActiveButton> _levelActiveButtons;
        private readonly List<ViewSlotPass> _viewSlotPasses = new();

        private Dictionary<PassType, List<Reddot>> _reddotDic;
        private bool _hasLastPassRewardInThisPassType;
        

        public ControllerCanvasGrowPass(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasGrowPass>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            
            _passListGroup = new ButtonRadioGroup();
            _passListGroup.onBindSelected += UpdateLevelView;
            foreach (var listButton in View.PassListButtons)
            {
                _passListGroup.AddListener(listButton);
            }

            _levelGroup = new ButtonRadioGroup();
            _levelGroup.AddListener(View.LevelActiveButton);
            _levelGroup.onBindSelected += (level) => UpdateGoalView((PassType)_passListGroup.SelectedIndex, level);
            _levelActiveButtons = new List<ActiveButton> { View.LevelActiveButton };
            
            InitReddot();
            
            View.PurchasePassButton.Button.onClick.AddListener(PurchasePass);
            View.CloseButton.onClick.AddListener(Close);
            View.ReceiveAllButton.OnClick.AddListener(() => GetRewardAll((PassType)_passListGroup.SelectedIndex, _levelGroup.SelectedIndex));
            View.GoalScrollRect.onValueChanged.AddListener(OnGoalScrollRectValueChange);

            StageManager.Instance.onBindStageClear += stageType =>
            {
                if (stageType == StageType.Normal)
                {
                    UpdateReddotByGoal(PassType.StagePass);
                }
            };
            DataController.Instance.elemental.OnBindChangeSummonCount += () => UpdateReddotByGoal(PassType.ElementalPass);
            DataController.Instance.rune.OnBindChangeSummonCount += () => UpdateReddotByGoal(PassType.RunePass);
        }

        public override void Open()
        {
            base.Open();
            _passListGroup.Select(0);
        }

        private void OnGoalScrollRectValueChange(Vector2 vec2)
        {
            View.PrevLastReward.SetActive(IsActivePrevLastReward());
        }

        private bool IsActivePrevLastReward()
        {
            return !IsGoalScrolledToBottom() && !_hasLastPassRewardInThisPassType; 
        }

        private bool IsGoalScrolledToBottom()
        {
            return View.GoalScrollRect.verticalNormalizedPosition <= 0.01f;
        }

        private void PurchasePass()
        {
            var passType = (PassType)_passListGroup.SelectedIndex;
            var level = _levelGroup.SelectedIndex;
            try
            {
                var iapType = GetIAPTypeFromPassType(passType, level);
                IAPManager.Instance.Purchase(iapType.ToString(), success =>
                {
                    if (success)
                    {
                        PassPurchased(passType, level);
                        UpdateGoalView(passType, level);
                    }
                    else 
                        Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                });
            }
            catch (Exception e)
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
            }
        }

        private void PassPurchased(PassType passType, int level)
        {
            DataController.Instance.growPass.SetPass(passType, level);
        }

        private void InitReddot()
        {
            _reddotDic = new Dictionary<PassType, List<Reddot>>();
            foreach (PassType passType in Enum.GetValues(typeof(PassType)))
            {
                var levelCount = DataController.Instance.growPass.GetLevelCount(passType);
                _reddotDic.Add(passType, new List<Reddot>());
                for (var i = 0; i < levelCount; ++i)
                {
                    _reddotDic[passType].Add(new Reddot(GetReddotTypeFromPassType(passType)));
                    UpdateReddot(passType, i);
                }
            }
        }

        private void UpdateReddotByGoal(PassType passType)
        {
            var goal = GetCurrGoalCount(passType);
            var level = DataController.Instance.growPass.FineLevelByGoal(passType, goal);
            if (level == -1) return;
            
            UpdateReddot(passType, level);
        }

        private void UpdateReddot(PassType passType, int level)
        {
            var goalCount = DataController.Instance.growPass.GetGoalCount(passType, level);
            var isOn = false;
            
            for (var i = 0; i < goalCount; ++i)
            {
                isOn = DataController.Instance.growPass.CanReceiveReward(passType, level, i, BattlePassType.Normal)
                           || DataController.Instance.growPass.CanReceiveReward(passType, level, i, BattlePassType.Premium);

                if (isOn) break;
            }
            _reddotDic[passType][level].IsOn = isOn;
            _reddotDic[passType][level].OnBindShowReddot();
        }

        private void UpdateLevelView(int passTypeIndex)
        {
            var passType = (PassType)passTypeIndex;
            var levelCount = DataController.Instance.growPass.GetLevelCount(passType);
            var i = _levelActiveButtons.Count;
            for (; i < levelCount; ++i)
            {
                var levelActiveButton = Object.Instantiate(View.LevelActiveButton, View.LevelParent);
                _levelGroup.AddListener(levelActiveButton);
                _levelActiveButtons.Add(levelActiveButton);
            }
            
            i = 0;
            for (; i < levelCount; ++i)
            {
                UpdateReddot(passType, i);
                
                _levelActiveButtons[i]
                    .SetActive(true)
                    .SetButtonText($"{GetPassTitle(passType)}{i + 1}");
                _levelActiveButtons[i].ReddotView.ShowReddot(IsReddotOn(passType, i));
                
            }
            
            for (; i < _levelActiveButtons.Count; ++i)
                _levelActiveButtons[i].SetActive(false);

            View.SetDescText(GetPassDesc(passType));
            View.GoalScrollRect.verticalNormalizedPosition = 1;
            
            _levelGroup.Select(0);
        }

        private void UpdateGoalView(PassType passType, int level)
        {
            var goalCount = DataController.Instance.growPass.GetGoalCount(passType, level);
            
            var i = _viewSlotPasses.Count;
            for (; i < goalCount; ++i)
            {
                var viewSlotPass = Object.Instantiate(View.ViewSlotPassPrefab, View.ViewSlotPassParent);
                var index = i;
                viewSlotPass.NViewGood.AddListener(() =>
                {
                    if (TryGetReward((PassType)_passListGroup.SelectedIndex, _levelGroup.SelectedIndex, index, BattlePassType.Normal))
                        UpdateGoalView((PassType)_passListGroup.SelectedIndex, _levelGroup.SelectedIndex);
                });
                viewSlotPass.PViewGood.AddListener(() =>
                {
                    if (TryGetReward((PassType)_passListGroup.SelectedIndex, _levelGroup.SelectedIndex, index, BattlePassType.Premium))
                        UpdateGoalView((PassType)_passListGroup.SelectedIndex, _levelGroup.SelectedIndex);
                });
                _viewSlotPasses.Add(viewSlotPass);
            }

            var hasPass = DataController.Instance.growPass.HasPass(passType, level);
            
            i = 0;
            for (; i < goalCount; ++i)
            {
                var growPass = DataController.Instance.growPass;
                var isNLock = growPass.IsLock(passType, level, i, BattlePassType.Normal);
                var isPLock = growPass.IsLock(passType, level, i, BattlePassType.Premium);
                var canReceiveNReward = growPass.CanReceiveReward(passType, level, i, BattlePassType.Normal);
                var canReceivePReward = growPass.CanReceiveReward(passType, level, i, BattlePassType.Premium);
                _viewSlotPasses[i]
                    .SetLevel(GetGoalCount(passType, level, i))
                    .ShowReddot(canReceiveNReward, canReceivePReward)
                    .SetActive(true);
                _viewSlotPasses[i].NViewGood
                    .SetInit(growPass.GetRewardType(passType, level, i, BattlePassType.Normal))
                    .SetValue(growPass.GetRewardValue(passType, level, i, BattlePassType.Normal), growPass.GetRewardParam0(passType, level, i, BattlePassType.Normal))
                    .SetActiveLockPaenl(isNLock)
                    .SetActiveCheckPanel(growPass.HasReward(passType, level, i ,BattlePassType.Normal));
                _viewSlotPasses[i].PViewGood
                    .SetInit(growPass.GetRewardType(passType, level, i, BattlePassType.Premium))
                    .SetValue(growPass.GetRewardValue(passType, level, i, BattlePassType.Premium), growPass.GetRewardParam0(passType, level, i, BattlePassType.Premium))
                    .SetActiveLockPaenl(isPLock)
                    .SetActiveCheckPanel(growPass.HasReward(passType, level, i ,BattlePassType.Premium));
                
                if (i + 1 == goalCount)
                {
                    _viewSlotPasses[i].PViewGood
                        .SetActiveLockPaenl(false)
                        .PlayTwincle();
                    View.PrevLastRewardViewGood
                        .SetInit(growPass.GetRewardType(passType, level, i, BattlePassType.Premium))
                        .SetValue(growPass.GetRewardValue(passType, level, i, BattlePassType.Premium), growPass.GetRewardParam0(passType, level, i, BattlePassType.Premium));

                    _hasLastPassRewardInThisPassType = growPass.HasReward(passType, level, i, BattlePassType.Premium);
                    View.PrevLastReward.SetActive(IsActivePrevLastReward());
                }
                else
                    _viewSlotPasses[i].PViewGood.StopTwincle();
            }
            for (; i < _viewSlotPasses.Count; ++i)
                _viewSlotPasses[i].SetActive(false);
            
            string price;
            var multiplePrice = "";
            try
            {
                var iapType = GetIAPTypeFromPassType(passType, level);
                price = IAPManager.Instance.GetPrice(iapType.ToString());
                multiplePrice = IAPManager.Instance.GetMultiplePriceString(iapType.ToString(), 3);
            }
            catch (Exception e)
            {
                price = "0";
            }

            UpdateReddot(passType, level);
            var isReddotOn = IsReddotOn(passType, level);
            _levelActiveButtons[level].ReddotView.ShowReddot(isReddotOn);
            View.ReceiveAllButton.Selected(isReddotOn);

            View
                .SetPrevLastRewardText(GetLastRewardDesc(passType, level))
                .SetCurrGoalCount(GetCurrGoalCountToString(passType))
                .SetPriceText(price)
                .SetMultiplePrice(multiplePrice)
                .SetAccountPurchaseCount(LocalizeManager.GetText(LocalizedTextType.AccountPurchase, hasPass ? 0 : 1, 1));
            View.PurchasePassButton.Selected(!hasPass);
            
            AutoScroll();
        }

        private void GetRewardAll(PassType passType, int level)
        {
            var isEffect = true;
            var goalCount = DataController.Instance.growPass.GetGoalCount(passType, level);
            for (var i = 0; i < goalCount; ++i)
            {
                if (TryGetReward(passType, level, i, BattlePassType.Normal, isEffect)) isEffect = false;
                if (TryGetReward(passType, level, i, BattlePassType.Premium, isEffect)) isEffect = false;
            }
            
            UpdateGoalView(passType, level);
        }

        private bool TryGetReward(PassType passType, int level, int index, BattlePassType battlePassType, bool isShowEffeect = true)
        {
            if (!DataController.Instance.growPass.CanReceiveReward(passType, level, index, battlePassType)) return false;
            
            var rewardGoodType = DataController.Instance.growPass.GetRewardType(passType, level, index, battlePassType);
            var rewardValue = DataController.Instance.growPass.GetRewardValue(passType, level, index, battlePassType);
            var rewardParam0 =DataController.Instance.growPass.GetRewardParam0(passType, level, index, battlePassType);

            DataController.Instance.growPass.SetReceive(passType, level, index, battlePassType);
            DataController.Instance.good.EarnReward(rewardGoodType, rewardValue, rewardParam0);
            
            if (isShowEffeect)
                GoodsEffectManager.Instance.ShowEffect(rewardGoodType, Vector2.zero, null, Mathf.Min((int)rewardValue, 10));
            
            DataController.Instance.LocalSave();
            return true;
        }

        private bool IsReddotOn(PassType passType, int level)
        {
            return _reddotDic[passType][level].IsOn;
        }

        private ReddotType GetReddotTypeFromPassType(PassType passType)
        {
            return passType switch
            {
                PassType.StagePass => ReddotType.StagePass,
                PassType.ElementalPass => ReddotType.ElementalPass,
                PassType.RunePass => ReddotType.RunePass,
                _ => ReddotType.None
            };
        }

        private int GetCurrGoalCount(PassType passType)
        {
            return passType switch
            {
                PassType.StagePass => DataController.Instance.stage.MaxTotalLevel,
                PassType.ElementalPass => DataController.Instance.elemental.summonCount,
                PassType.RunePass => DataController.Instance.rune.summonCount,
                _ => 0
            };
        }

        private string GetCurrGoalCountToString(PassType passType)
        {
            return passType switch
            {
                PassType.StagePass => LocalizeManager.GetText(LocalizedTextType.GrowPass_CurrStage, DataController.Instance.stage.MaxStageToString),
                PassType.ElementalPass => LocalizeManager.GetText(LocalizedTextType.GrowPass_CurrElementalSummon, DataController.Instance.elemental.summonCount),
                PassType.RunePass => LocalizeManager.GetText(LocalizedTextType.GrowPass_CurrRuneSummon, DataController.Instance.rune.summonCount),
                _ => LocalizeManager.GetText(LocalizedTextType.GrowPass_CurrStage),
            };
        }

        private string GetLastRewardDesc(PassType passType, int level)
        {
            return passType switch
            {
                PassType.StagePass => LocalizeManager.GetText(LocalizedTextType.GrowPass_StageLastRewardDesc,
                    DataController.Instance.growPass.GetLastRewardValue(passType, level).ToGoodString(GoodType.Gold)),
                PassType.ElementalPass => LocalizeManager.GetText(LocalizedTextType.GrowPass_ElementalSummonLastRewardDesc),
                PassType.RunePass => LocalizeManager.GetText(LocalizedTextType.GrowPass_RuneSummonLastRewardDesc),
                _ => LocalizeManager.GetText(LocalizedTextType.GrowPass_GoalStage),
            };
        }

        private string GetGoalCount(PassType passType, int level, int index)
        {
            var goal = DataController.Instance.growPass.GetGoal(passType, level, index);
            return passType switch
            {
                PassType.StagePass => LocalizeManager.GetText(LocalizedTextType.GrowPass_GoalStage, DataController.Instance.stage.GetStageLevelExpression(goal)),
                PassType.ElementalPass => LocalizeManager.GetText(LocalizedTextType.GrowPass_GoalElementalSummon, goal),
                PassType.RunePass => LocalizeManager.GetText(LocalizedTextType.GrowPass_GoalRuneSummon, goal),
                _ => LocalizeManager.GetText(LocalizedTextType.GrowPass_GoalStage),
            };
        }

        private string GetPassTitle(PassType passType)
        {
            return passType switch
            {
                PassType.StagePass => LocalizeManager.GetText(LocalizedTextType.StagePassTitle),
                PassType.ElementalPass => LocalizeManager.GetText(LocalizedTextType.ElementalPassTitle),
                PassType.RunePass => LocalizeManager.GetText(LocalizedTextType.RunePassTitle),
                _ => LocalizeManager.GetText(LocalizedTextType.StagePassTitle),
            };
        }

        private string GetPassDesc(PassType passType)
        {
            return passType switch
            {
                PassType.StagePass => LocalizeManager.GetText(LocalizedTextType.StagePassDesc),
                PassType.ElementalPass => LocalizeManager.GetText(LocalizedTextType.ElementalPassDesc),
                PassType.RunePass => LocalizeManager.GetText(LocalizedTextType.RunePassDesc),
                _ => LocalizeManager.GetText(LocalizedTextType.StagePassDesc),
            };
        }

        private IAPType GetIAPTypeFromPassType(PassType passType, int level)
        {
            var iapTypeToString = passType switch
            {
                PassType.StagePass => IAPType.net_themessage_etd_stagepass0.ToString(),
                PassType.ElementalPass => IAPType.net_themessage_etd_elementalsummonpass0.ToString(),
                PassType.RunePass => IAPType.net_themessage_etd_runesummonpass0.ToString(),
            };

            Span<char> span = iapTypeToString.ToCharArray();
            var newChar = level.ToString();
            span[^1] = newChar[0];
            iapTypeToString = new string(span);

            if (Enum.TryParse(typeof(IAPType), iapTypeToString, out var result))
            {
                return (IAPType)result;
            }

            throw new InvalidOperationException("InvalidOperationException Type IAPType");
        }

        private void AutoScroll()
        {
            var passType = (PassType)_passListGroup.SelectedIndex;
            for (var i = 0; i < _viewSlotPasses.Count; ++i)
            {
                if (_viewSlotPasses[i].isActiveAndEnabled) break;
                if (DataController.Instance.growPass.CanReceiveReward(passType, _levelGroup.SelectedIndex, i, BattlePassType.Normal) ||
                    DataController.Instance.growPass.CanReceiveReward(passType, _levelGroup.SelectedIndex, i, BattlePassType.Premium))
                {
                    Utility.ScrollKill();
                    Utility.ScrollToTarget(View.GoalScrollRect, i, 5f).Forget();
                    break;
                }
            }
        }
    }
}