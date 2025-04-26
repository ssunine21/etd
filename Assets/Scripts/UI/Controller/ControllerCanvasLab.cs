using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasLab : ControllerCanvas
    {
        private readonly Dictionary<ResearchType, ViewSlotResearch> _viewSlotResearches;
        private readonly List<ViewSlotLab> _viewSlotLabs;
        
        private ViewCanvasLab View => ViewCanvas as ViewCanvasLab;
        private Vector2 _cellSize = Vector2.zero;
        
        private ResearchType _currSelectedResearchType = ResearchType.None;
        
        public ControllerCanvasLab(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasLab>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.ResearchViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.SepcialLabCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            
            _viewSlotResearches = new Dictionary<ResearchType, ViewSlotResearch>();
            _viewSlotLabs = new List<ViewSlotLab> { View.ViewSlotSpecialLab };
            for (var i = 0; i < DataController.Instance.research.GetLabCount(); ++i)
            {
                _viewSlotLabs.Add(InstantiateLabSlot(View.ViewSlotLabParent));
            }

            for (var i = 0; i < _viewSlotLabs.Count; ++i)
            {
                var index = i;
                _viewSlotLabs[i].ShowInfo.onClick.AddListener(() =>
                {
                    var researchType = DataController.Instance.research.GetResearchTypeByLabSlotIndex(index);
                    if (DataController.Instance.research.IsResearchTimeComplete(researchType))
                    {
                        ResearchComplete(researchType);
                    }
                    else
                        OpenViewPopup(researchType);
                });
            }
            
            View.ResearchGourp.AddListener((index) =>
            {
                foreach (var re0 in View.ResearchGroup0) re0.SetActive(index == 0);
                foreach (var re1 in View.ResearchGroup1) re1.SetActive(index == 1);
                foreach (var re2 in View.ResearchGroup2) re2.SetActive(index == 2);
            });
            View.LabType.AddListener((index) =>
            {
                for (var i = 0; i < View.LabTypePanels.Length; ++i)
                    View.LabTypePanels[i].SetActive(index == i);
                View.ResearchGourp.gameObject.SetActive(index == 0);
                
                if(index == 0)
                    View.ResearchGourp.OnClick(0);
            });
            
            View.ViewSlotResearchPopupView.ResearchButton.onClick.AddListener(() => TryResearch(_currSelectedResearchType));
            View.CompleteImmediately.onClick.AddListener(() => ResearchComplete(_currSelectedResearchType));
            View.ResearchCancel.onClick.AddListener(() =>
            {
                Get<ControllerCanvasToastMessage>().SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.Warring),
                    LocalizeManager.GetText(LocalizedTextType.Research_Cancel_Desc),
                    LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                    LocalizeManager.GetText(LocalizedTextType.Confirm), () =>
                    {
                        ResearchCancel(_currSelectedResearchType);
                    }).ShowToastMessage();
            });
            
            View.CompleteImmediatelyByDia.onClick.AddListener(() =>
            {
                var diaCost = DataController.Instance.research.GetDiaForImmediatelyComplete(_currSelectedResearchType);
                Get<ControllerCanvasToastMessage>().SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.Warring),
                    LocalizeManager.GetText(LocalizedTextType.Research_ImmediatelyCompleteCheck, diaCost),
                    LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                    LocalizeManager.GetText(LocalizedTextType.Confirm), () =>
                    {
                        if (DataController.Instance.good.TryConsume(GoodType.Dia, diaCost))
                        {
                            ResearchComplete(_currSelectedResearchType);
                        }
                    }).ShowToastMessage();
            });
            
            View.OpenSpecialLabPopup.onClick.AddListener(() => View.SepcialLabCanvasPopup.Open());
            View.PurchaseSpecialLab.onClick.AddListener(PurchaseSpecialLab);
            View.CloseButton.onClick.AddListener(Close);

            InitStorage();
            
            UpdateLabSlotCount();
            UpdateResearchSlot();

            for (var i = 0; i < _viewSlotLabs.Count; ++i)
            {
                var researchType = DataController.Instance.research.GetResearchTypeByLabSlotIndex(i);
                UpdateLabSlot(researchType);
            }

            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseLabSlot] += UpdateLabSlotCount;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseResearchSpeed] += UpdateResearchSlot;
            DataController.Instance.research.OnBindResearch[ResearchType.DecreaseResearchCost] += UpdateResearchSlot;

            TimeTask().Forget();
        }

        public GameObject GetTutorialObject(TutorialType tutorialType, int param0)
        {
            if (tutorialType == TutorialType.Research)
            {
                return param0 switch
                {
                    0 => View.TutorialLabPanel,
                    1 => View.TutorialStoragePanel,
                    2 => View.TutorialSaveToStoragePanel,
                    3 => View.StorageTutorialPanel0,
                    4 => View.StorageTutorialPanel1,
                    _ => null,
                };
            }
            return null;
        }

        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => IAPManager.Instance.IsInitialized);
            View.SetSpecialLabPrice(IAPManager.Instance.GetPrice(IAPType.net_themessage_etd_speciallab.ToString()));
            
            while (true)
            {
                for (var i = 0; i < _viewSlotLabs.Count; ++i)
                {
                    if (!_viewSlotLabs[i].IsEmptySlot && !_viewSlotLabs[i].IsLockSlot)
                    {
                        UpdateLabSlotTimestampAndReddot(i);
                    }
                }

                if (View.ResearchViewCanvasPopup.isActiveAndEnabled
                    && DataController.Instance.research.IsResearching(_currSelectedResearchType))
                    UpdateViewPopupTimestamp(_currSelectedResearchType);
                
                UpdateProtectTime();
                
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            }
        }
        
        public override void Open()
        {
            base.Open();
            NormalizeGridLayoutSize();
            View.LabType.OnClick(0);
        }
        
        private void PurchaseSpecialLab()
        {
            IAPManager.Instance.Purchase(IAPType.net_themessage_etd_speciallab.ToString(), (isSuccess) =>
            {
                if (isSuccess)
                {
                    DataController.Instance.good.Earn(GoodType.SpecialLab, 1);
                    UpdateLabSlotCount();
                    UpdateLabSlot(0);
                }
                DataController.Instance.SaveBackendData();
                View.SepcialLabCanvasPopup.Close();
            });
        }

        private void ResearchCancel(ResearchType researchType)
        {
            var index = DataController.Instance.research.GetLabSlotIndex(researchType);
            DataController.Instance.research.CancelLabSlot(researchType);
            
            UpdateViewPopup(researchType);
            UpdateLabSlot(index);
        }

        private bool TryResearch(ResearchType researchType)
        {
            var currLevel = DataController.Instance.research.GetCurrLevel(researchType);
            var cost = DataController.Instance.research.GetCost(researchType, currLevel);

            if (DataController.Instance.research.IsMaxLevel(researchType))
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NoMoreResearch);
                return false;
            }
            
            if (DataController.Instance.good.TryConsume(GoodType.DarkDia, cost))
            {
                if (TryGetLabSlot(out var index))
                {
                    Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Research_Start);
                    DataController.Instance.research.SetLabSlot(index, researchType, _viewSlotLabs[index].IsSpecialLab);

                    UpdateViewPopup(DataController.Instance.research.GetResearchTypeByLabSlotIndex(index));
                    UpdateDiaForImmediatelyComplete(researchType);
                    UpdateViewSlotResearch(researchType);
                    UpdateLabSlot(researchType);
                    
                    return true;
                }
                Get<ControllerCanvasToastMessage>()
                    .ShowTransientToastMessage(LocalizedTextType.Research_NotEnoughLabSlot);
            }

            return false;
        }

        private void UpdateLabSlotCount()
        {
            var labSlotCount = DataController.Instance.research.GetValue(ResearchType.IncreaseLabSlot);

            _viewSlotLabs[0].SetLock(DataController.Instance.good.GetValue(GoodType.SpecialLab) < 1);
            for (var i = 0; i < DataController.Instance.research.GetLabCount(); ++i)
            {
                _viewSlotLabs[i + 1].SetLock(i >= labSlotCount);
            }
        }

        private void UpdateLabSlot(int index)
        {
            if (index == -1) return;
            
            var slot = _viewSlotLabs[index];
            var researchType = DataController.Instance.research.GetResearchTypeByLabSlotIndex(index);

            if (researchType == ResearchType.None)
                slot.SetEnable(false);
            else
                UpdateLabSlot(researchType);
        }

        private void UpdateLabSlot(ResearchType researchType)
        {
            var index = DataController.Instance.research.GetLabSlotIndex(researchType);
            if (index == -1) return;
            
            var slot = _viewSlotLabs[index];
            if (DataController.Instance.research.IsResearching(researchType))
            {
                var localizeType = DataController.Instance.research.GetLocalizedTextType(researchType);
                slot
                    .SetTitle($"{LocalizeManager.GetText(localizeType)}")
                    .SetEnable(true);
            }
            else
                slot.SetEnable(false);
        }

        private void UpdateResearchSlot()
        {
            foreach (var bData in DataController.Instance.research.BDatas)
            {
                var researchType = bData.researchType;
                var bDataIndex = Mathf.Clamp(bData.slotIndex, 0, View.ResearchLayoutGroups.Length - 1);
                var slot = GetResearchSlot(bData.researchType, View.ResearchLayoutGroups[bDataIndex].transform);
                
                UpdateViewSlotResearch(researchType, slot);
            }
        }

        private void UpdateViewSlotResearch(ResearchType type, ViewSlotResearch viewSlotResearch = null)
        {
            if (!viewSlotResearch) viewSlotResearch = _viewSlotResearches[type];
            
            var isMaxLevel = DataController.Instance.research.IsMaxLevel(type);
            var currLevel = DataController.Instance.research.GetCurrLevel(type);
            var currLevelText = isMaxLevel
                ? LocalizeManager.GetText(LocalizedTextType.Lv, LocalizeManager.GetText(LocalizedTextType.UpgradeMaxTitle))
                : LocalizeManager.GetText(LocalizedTextType.Lv, currLevel) 
                  + " <size=70%><sprite=12></size> " 
                  + $"<color=orange>{currLevel + 1}</color>";

            var currValue = DataController.Instance.research.GetValue(type, currLevel);
            var nextValue = DataController.Instance.research.GetValue(type, currLevel + 1);
            var currValueText = string.Format(DataController.Instance.research.GetExpression(type), currValue)
                .Replace(" ", "");
            var nextValueText = string.Format(DataController.Instance.research.GetExpression(type), nextValue)
                .Replace(" ", "");
            var valueText = isMaxLevel
                ? $"<color=orange>({currValueText})</color>"
                : $"<color=orange>({currValueText} <size=70%><sprite=12></size> {nextValueText})</color>";

            var timeSec = DataController.Instance.research.GetResearchTime(type, currLevel);
            var timestamp = Utility.GetTimeStringToFromTotalSecond(timeSec);

            var localizeType = DataController.Instance.research.GetLocalizedTextType(type);
            var localizeDescType =
                Enum.TryParse(typeof(LocalizedTextType), localizeType + "_Desc", out var descType) 
                    ? (LocalizedTextType)descType : localizeType;
            
            var cost = DataController.Instance.research.GetCost(type, currLevel);

            viewSlotResearch
                .SetTitle($"{LocalizeManager.GetText(localizeType)}")
                .SetDescription(LocalizeManager.GetText(localizeDescType))
                .SetValue($"{currLevelText} {valueText}")
                .SetTimestamp(timestamp)
                .SetMaxLevel(isMaxLevel);

            viewSlotResearch.ViewGood
                .SetInit(GoodType.DarkDia)
                .SetValue(cost);

            if (viewSlotResearch.ResearchingPanel)
            {
                viewSlotResearch.ResearchingPanel.SetActive(DataController.Instance.research.IsResearching(type));
            }
        }

        private ViewSlotResearch GetResearchSlot(ResearchType type, Transform parent)
        {
            if(!_viewSlotResearches.ContainsKey(type))
            {
                var slot = InstantiateResearchSlot(type, parent);
                _viewSlotResearches.TryAdd(type, slot);
            }

            return _viewSlotResearches[type].SetActive(true);
        }

        private bool TryGetLabSlot(out int index)
        {
            index = -1;
            for (var i = 0; i < _viewSlotLabs.Count; ++i)
            {
                if (_viewSlotLabs[i].IsEmptySlot)
                {
                    index = i;
                    break;
                }
            }
            return index > -1;
        }

        private ViewSlotLab InstantiateLabSlot(Transform parent)
        {
            var slot = Object.Instantiate(View.ViewSlotLabPrefab, parent);
            var index = _viewSlotLabs.Count;
            slot.SetLabTitle($"Lab {index}");
            slot.SetActive(true);
            return slot;
        }
        
        private void ResearchComplete(ResearchType researchType)
        {
            var index = DataController.Instance.research.GetLabSlotIndex(researchType);
            _viewSlotLabs[index].Reddot.ShowReddot(false);
            DataController.Instance.research.ResearchComplete(researchType);
            
            UpdateViewPopup(researchType);
            UpdateViewSlotResearch(researchType, View.ViewSlotResearchPopupView);
            UpdateViewSlotResearch(researchType);
            UpdateLabSlot(index);
            
            
            var currLevel = DataController.Instance.research.GetCurrLevel(researchType);
            var currLevelText = LocalizeManager.GetText(LocalizedTextType.Lv, currLevel);
            var localizeType = DataController.Instance.research.GetLocalizedTextType(researchType);
            var text = $"{LocalizeManager.GetText(localizeType)} {currLevelText}";
            Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(
                LocalizeManager.GetText(LocalizedTextType.Research_Complete, text));
        }

        private ViewSlotResearch InstantiateResearchSlot(ResearchType type, Transform parent)
        {
            var slot = Object.Instantiate(View.ViewSlotResearchPrefab, parent);
            slot.ResearchButton.onClick.AddListener(() =>
            {
                if (DataController.Instance.research.IsMaxLevel(type))
                {
                    Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NoMoreResearch);
                }
                else
                {
                    OpenViewPopup(type);
                }
            });
            return slot;
        }

        private void OpenViewPopup(ResearchType researchType)
        {
            _currSelectedResearchType = researchType;
            UpdateViewPopup(researchType);
            UpdateViewSlotResearch(researchType, View.ViewSlotResearchPopupView);
            UpdateDiaForImmediatelyComplete(researchType);
            View.ResearchViewCanvasPopup.Open();
        }
        
        private void NormalizeGridLayoutSize()
        {
            if (_cellSize != Vector2.zero) return;
            
            var width = View.ResearchRectTransforms[0].rect.width - (View.ResearchLayoutGroups[0].spacing.x * 3);
            var cellXSize = width / 2;
            _cellSize = new Vector2(cellXSize, cellXSize * 0.65f);
            
            foreach (var researchLayoutGroup in View.ResearchLayoutGroups)
            {
                researchLayoutGroup.cellSize = _cellSize;
            }
        }

        private void UpdateViewPopup(ResearchType researchType)
        {
            var isResearching = DataController.Instance.research.IsResearching(researchType);
            ChangeResearchButtonPaenl(isResearching);

            if (isResearching)
            {
                UpdateViewPopupTimestamp(researchType);
            }
        }

        private void ChangeResearchButtonPaenl(bool isResearching)
        {
            View.BeforeResearchPaenl.SetActive(!isResearching);
            View.AfterResearchPaenl.SetActive(isResearching);
            View.TimePanelAtViewPopup.SetActive(isResearching);
        }

        private void UpdateViewPopupTimestamp(ResearchType researchType)
        {
            GetTimeInfo(researchType, out var remainTimeToString, out var fillAmount);
            View.TimeFillAmount.fillAmount = fillAmount;
            View.TimeTMP.text = remainTimeToString;
            
            UpdateCompleteButton(researchType);
        }

        private void UpdateCompleteButton(ResearchType researchType)
        {          
            var isResearchTimeComplete = DataController.Instance.research.IsResearchTimeComplete(researchType);
            View.CompleteImmediatelyByDia.gameObject.SetActive(!isResearchTimeComplete);
            View.CompleteImmediately.gameObject.SetActive(isResearchTimeComplete);
        }

        private void UpdateDiaForImmediatelyComplete(ResearchType researchType)
        {
            var goodValue = DataController.Instance.research.GetDiaForImmediatelyComplete(researchType);
            View.DiaPerSecondViewGood.SetValue(goodValue);
        }

        private void UpdateLabSlotTimestampAndReddot(int labSlotIndex)
        {
            var researchType = DataController.Instance.research.GetResearchTypeByLabSlotIndex(labSlotIndex);
            var isResearchTimeComplete = DataController.Instance.research.IsResearchTimeComplete(researchType);
            var infoText = isResearchTimeComplete
                ? LocalizeManager.GetText(LocalizedTextType.CompleteImmediately)
                : LocalizeManager.GetText(LocalizedTextType.ShowInfo);

            _viewSlotLabs[labSlotIndex].Reddot.ShowReddot(isResearchTimeComplete);
            
            GetTimeInfo(researchType, out var timeText, out var fillAmount);
            _viewSlotLabs[labSlotIndex]
                .SetTime(timeText)
                .SetFillAmount(fillAmount)
                .SetInfoText(infoText);

            _viewSlotLabs[labSlotIndex].LoadingIcon.gameObject.SetActive(!isResearchTimeComplete);
            
        }

        private void GetTimeInfo(ResearchType researchType, out string timeText, out float fillAmount)
        {
            var timeSec = DataController.Instance.research.GetResearchTime(researchType);
            var endTimeToString = DataController.Instance.research.GetResearchEndTime(researchType);
            var remainTime = ServerTime.RemainingTimeToTimeSpan(endTimeToString);
            timeText = Utility.GetTimeStringToFromTotalSecond(remainTime);

            try
            {
                fillAmount = (float)(timeSec - remainTime.TotalSeconds) / timeSec;
            }
            catch (Exception e)
            {
                fillAmount = 0;
            }
        }
    }
}