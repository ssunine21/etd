using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasStage : ControllerCanvas
    {
        private readonly List<ViewSlotStageSelect> _viewSlotStageSelects = new();
        private const string ViewSlotSelectName = nameof(ViewSlotStageSelect);

        private ViewCanvasStage View => ViewCanvas as ViewCanvasStage;
        private int originCanvasSortingOrder;

        private bool _isAsyncScroll;
        private bool _isAutoScrolling;
        private int _lastStageLevelScrollIndex;
        private int _currStageListViewIndex;

        public ControllerCanvasStage(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasStage>())
        {
            View.SetActive(true);
            View.StageListViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            originCanvasSortingOrder = ViewCanvas.Canvas.sortingOrder;
            
            View.StageListButton.onClick.AddListener(() =>
            {
                AsyncScrollView(DataController.Instance.stage.currTotalLevel);
                View.StageListViewCanvasPopup.Open();
                SetForwardBottomMenuSortingOrder();
            });

            View.GoToMaxStage.onClick.AddListener(() =>
            {
                if (DataController.Instance.stage.IsMaxStage) return;
                StageManager.Instance.MoveToNormalStageLevel(DataController.Instance.stage.MaxTotalLevel);
            });

            View.GoCurrStageButton.onClick.AddListener(() => AsyncScrollView(DataController.Instance.stage.currTotalLevel));
            
            View.NextStageButton.onClick.AddListener(() => UpdateStageListView(_currStageListViewIndex + 1));
            View.PrevStageButton.onClick.AddListener(() => UpdateStageListView(_currStageListViewIndex - 1));
            
            View.StageListViewCanvasPopup.Close();

            StageManager.Instance.onBindChangeStageType += UpdateChangeStage;
            StageManager.Instance.onBindStartStage += (type, level) =>
            {
                if (type == StageType.Normal)
                    UpdateView();
            };

            UpdateView();
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

        private void AsyncScrollView(int targetTotalLevel)
        {
            UpdateStageListView(targetTotalLevel / DataController.Instance.stage.StageSpacing);
            UpdateScrollToTarget(targetTotalLevel % DataController.Instance.stage.StageSpacing);
        }

        private void UpdateView()
        {
            var fillAmountValue =
                (float)(DataController.Instance.stage.MaxSubLevel) / (DataController.Instance.stage.StageSpacing - 1);

            View
                .SetStageText(DataController.Instance.stage.CurrStageToFullSring)
                .SetActiveRepeatArrow(!DataController.Instance.stage.IsMaxStage)
                .SetStageView(DataController.Instance.stage.IsMaxStage, DataController.Instance.stage.IsMaxStageTheBossStage);

            View.ProgressFillAmount
                .DOFillAmount(fillAmountValue, 0.7f)
                .SetEase(Ease.OutCirc)
                .SetUpdate(true);

            var rawImageIndex = DataController.Instance.stage.IsMaxStageTheBossStage ? 1 : 0;
            View.BackgroundRolling.SetRawImage(rawImageIndex);
        }

        private void UpdateScrollToTarget(int subLevel)
        {
            Utility.ScrollToTarget(View.StageScrollRect, subLevel, 5f).Forget();
        }

        private void UpdateStageListView(int mainLevel)
        {
            _currStageListViewIndex = mainLevel;
            var totalLevel = mainLevel * DataController.Instance.stage.StageSpacing + (DataController.Instance.stage.StageSpacing - 1);
            View
                .SetStageListTitle(DataController.Instance.stage.GetStageTitle(mainLevel))
                .SetStageListLevelTitle($"{DataController.Instance.stage.GetLevelAndStepText(mainLevel)}")
                .SetStageListExpression(DataController.Instance.stage.GetStageLevelExpression(totalLevel));
            
            UpdateSlots(mainLevel);
        }

        private void UpdateSlots(int mainLevel)
        {
            View.PrevStageButton.gameObject.SetActive(mainLevel > 0);
            View.NextStageButton.gameObject.SetActive(mainLevel < DataController.Instance.stage.BDatas.Length - 1);
            
            var data = DataController.Instance.stage.GetBData(mainLevel);
            if (data == null) return;
            
            var currTotalLevel = DataController.Instance.stage.currTotalLevel;
            var maxTotalLevel = DataController.Instance.stage.MaxTotalLevel;

            var viewSlot = _viewSlotStageSelects.GetViewSlots(ViewSlotSelectName, View.StageScrollRect.content, 30);

            var i = 0;
            foreach (var slot in viewSlot)
            {
                var totalLevel = mainLevel * DataController.Instance.stage.StageSpacing + i++;
                slot
                    .SetStageLevel(mainLevel)
                    .SetStageLevelText(DataController.Instance.stage.GetStageLevelExpression(totalLevel))
                    .SetGoodValue(data.rewardTypes, data.rewardValues)
                    .SetSelectedSlot(totalLevel == currTotalLevel, totalLevel > maxTotalLevel)
                    .SetActive(true);
                
                if(!slot.IsInit)
                {
                    slot.AddGoToStageButtonAction(() =>
                    {
                        View.StageListViewCanvasPopup.Close();
                        SetOriginSortingOrder();
                        StageManager.Instance.MoveToNormalStageLevel(slot.StageLevel);
                    });
                    slot.IsInit = true;
                }
            }
        }

        private void UpdateChangeStage(StageType stageType, int param0)
        {
            View.SetActive(stageType == StageType.Normal);
        }
    }
}