using System.Linq;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasDungeon
    {
        private StageType _selectedStageType;
        private int originCanvasSortingOrder;
        
        private void InitEnterPanel()
        {
            View.EnterPanel.SetViewAnimation(ViewAnimationType.SlideUp);
            
            View.EnterPanel.Close();
            originCanvasSortingOrder = ViewCanvas.Canvas.sortingOrder;
            
            View.EnterButton.onClick.AddListener(TryEnterDungeon);
            View.SweepConfirmButton.onClick.AddListener(TrySweep);

            View.SweepButton.onClick.AddListener(ShowSweepPanel);
            View.CloseButton.onClick.AddListener(() => View.EnterPanel.Close());
            View.CountButton.OnCountChanged += UpdateSweepReward;
            View.NextChallengeCheckBox.Toggle.onValueChanged.AddListener(isOn => DataController.Instance.setting.dungeonNextChallenge = isOn);

            foreach (var sweepCloseButtons in View.SweepCloseButtons)
            {
                sweepCloseButtons.onClick.AddListener(() => ShowSweepPanel(false));
            }

            View.EnterPanel.OnBindOpen += SetForwardBottomMenuSortingOrder;
            View.EnterPanel.OnBindClose += SetOriginSortingOrder;
            
            StageManager.Instance.onBindStageEnd += (stageType, _) => UpdateEnterView(stageType);
            
            ShowSweepPanel(false);
        }

        private void SetForwardBottomMenuSortingOrder()
        {
            var bottomSortingOrder = UI.ViewCanvas.ViewCanvas.Get<ViewCanvasBottomMenu>().Canvas.sortingOrder;
            View.Canvas.sortingOrder = bottomSortingOrder + 1;

        }

        private void SetOriginSortingOrder()
        {
            View.Canvas.sortingOrder = originCanvasSortingOrder;
        }

        private void TryEnterDungeon()
        {
            if (DataController.Instance.good.GetValue(DataController.Instance.dungeon.GetTicketGoodType(_selectedStageType)) > 0)
                Get<ControllerCanvasToastMessage>().ShowFadeOutIn(() => StageManager.Instance.ChangeStage(_selectedStageType));
            else
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
        }

        private void ShowSweepPanel()
        {
            if (DataController.Instance.good.GetValue(DataController.Instance.dungeon.GetTicketGoodType(_selectedStageType)) > 0)
            {            
                ShowSweepPanel(true);
            }
            else
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
        }

        private void TrySweep()
        {
            if(DataController.Instance.good.TryConsume(DataController.Instance.dungeon.GetTicketGoodType(_selectedStageType), View.CountButton.CurrCount))
            {
                foreach (var eanredRewardViewGood in View.SweepRewardViewGoods.Where(x => x.isActiveAndEnabled))
                {
                    var value = eanredRewardViewGood.GoodValue * DataController.Instance.dungeon.GetReduceValue();
                    DataController.Instance.good.Earn(eanredRewardViewGood.GoodType, value);
                }

                ShowClearView();
                ShowSweepPanel(false);

                var count = View.CountButton.CurrCount;
                switch (_selectedStageType)
                {
                    case StageType.GoldDungeon:
                        DataController.Instance.mission.Count(MissionType.ClearGoldDungeon, count);
                        DataController.Instance.mission.Count(MissionType.ClearAnyDungeon, count);
                        DataController.Instance.quest.Count(QuestType.ClearGoldDungeon, count);
                        break;
                    case StageType.DiaDungeon:
                        DataController.Instance.mission.Count(MissionType.ClearDiaDungeon, count);
                        DataController.Instance.mission.Count(MissionType.ClearAnyDungeon, count);
                        DataController.Instance.quest.Count(QuestType.ClearDiaDungeon, count);
                        break;
                    case StageType.EnhanceDungeon:
                        DataController.Instance.mission.Count(MissionType.ClearEnhanceDungeon, count);
                        DataController.Instance.mission.Count(MissionType.ClearAnyDungeon, count);
                        DataController.Instance.quest.Count(QuestType.ClearEnhanceDungeon, count);
                        return;
                }
            }
        }

        private void UpdateEnterView(StageType stageType)
        {
            if (stageType != StageType.DiaDungeon && stageType != StageType.GoldDungeon && stageType != StageType.EnhanceDungeon) return;

            var goodType = DataController.Instance.dungeon.GetTicketGoodType(stageType);
            if (goodType == GoodType.None) return;
            
            View
                .SetTitle(LocalizeManager.GetDungeonTitle(stageType))
                .SetDesc(LocalizeManager.GetDungeonDescription(stageType))
                .SetBannerSprite(ResourcesManager.Instance.dungeonBlurryBanners[(int)stageType])
                .SetMaxGoal(LocalizeManager.GetDungeonGoalTextWithCount(stageType,
                    DataController.Instance.dungeon.GetMaxGoalCount(stageType)))
                .SetNeedGood(goodType)
                .SetActiveNextChallenge(stageType == StageType.EnhanceDungeon);

            View.NextChallengeCheckBox.Toggle.SetIsOnWithoutNotify(DataController.Instance.setting.dungeonNextChallenge);

            var rewards = DataController.Instance.dungeon.GetLastEarnedRewards(stageType);
            var i = 0;
            foreach (var reward in rewards)
            {
                View.RewardViewGoods[i].gameObject.SetActive(true);
                View.RewardViewGoods[i]
                    .SetInit(reward.Key)
                    .SetValue(reward.Value)
                    .SetActiveValueText(reward.Value > 0);
                ++i;
            }

            for (; i < View.RewardViewGoods.Length; ++i)
            {
                View.RewardViewGoods[i].gameObject.SetActive(false);
            }

            _selectedStageType = stageType;
            
            View.CountButton.MaxCount = (int)DataController.Instance.good.GetValue(DataController.Instance.dungeon.GetTicketGoodType(stageType));
            View.CountButton.SetCount(1);

            View.SweepButton.gameObject.SetActive(DataController.Instance.dungeon.GetMaxGoalCount(stageType) > 0);
        }

        private void UpdateSweepReward(int sweepCount)
        { 
            var rewards = DataController.Instance.dungeon.GetLastEarnedRewards(_selectedStageType);
            var i = 0;
            foreach (var reward in rewards)
            {
                View.SweepRewardViewGoods[i].gameObject.SetActive(true);
                View.SweepRewardViewGoods[i].SetInit(reward.Key)
                    .SetValue(reward.Value * Mathf.Max(sweepCount, 1));
                ++i;
            }

            for (; i < View.RewardViewGoods.Length; ++i)
            {
                View.SweepRewardViewGoods[i].gameObject.SetActive(false);
            }

            View.SweepNeededRewardGood.SetValue(sweepCount);
        }

        private void ShowSweepPanel(bool flag)
        {
            var duration = 0.3f;
            if (flag)
            {

                View.SweepPanel.SetActive(true);

                View.SweepRectTransform.anchoredPosition = new Vector2(0, -800);
                View.SweepRectTransform
                    .DOAnchorPosY(-100, duration).SetEase(Ease.OutBack).SetUpdate(true);

                View.SweepCanvasGroup.alpha = 0;
                View.SweepCanvasGroup
                    .DOFade(1, duration)
                    .SetUpdate(true);

                View.SweepBackground.color = new Color(0, 0, 0, 0);
                View.SweepBackground
                    .DOFade(0.6f, duration + 0.1f).SetUpdate(true);
            }
            else
            {
                View.SweepRectTransform
                    .DOAnchorPosY(-800, duration).SetEase(Ease.InQuad).SetUpdate(true);
                View.SweepCanvasGroup
                    .DOFade(0, duration).SetUpdate(true);
                View.SweepBackground.DOFade(0f, duration).OnComplete(() => { View.SweepPanel.SetActive(false); })
                    .SetUpdate(true);
            }
        }

        private void ShowClearView()
        {
            var goodItems =
                (from viewGood in View.SweepRewardViewGoods
                    where viewGood.isActiveAndEnabled
                    select new GoodItem(viewGood.GoodType, viewGood.GoodValue * DataController.Instance.dungeon.GetReduceValue())).ToList();
            
            var controllerCanvasClear = Get<ControllerCanvasClear>();
            controllerCanvasClear
                .SetTitle(LocalizeManager.GetText(LocalizedTextType.ClearTitle))
                .SetMyGood(DataController.Instance.dungeon.GetTicketGoodType(_selectedStageType))
                .SetNeededRewards(GoodType.None)
                .SetAction(LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ExitButton), () => controllerCanvasClear.Close())
                .SetReduceType(ReduceType.Vip, (float)DataController.Instance.good.GetValue(GoodType.IncreaseDungeonReward))
                .SetReduceType(ReduceType.Research, DataController.Instance.research.GetValue(ResearchType.IncreaseDungeonReward))
                .SetRewardsAndShow(goodItems).Forget();
            
            controllerCanvasClear.PlayReduceAnimation().Forget();
        }
    }
}