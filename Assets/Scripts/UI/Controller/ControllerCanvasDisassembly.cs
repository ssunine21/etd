using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasDisassembly : ControllerCanvas
    {
        private readonly List<ViewSlotUI> _viewSlotUis = new();

        private ViewCanvasDisassembly View => ViewCanvas as ViewCanvasDisassembly;
        private double _currRewardValue;

        public ControllerCanvasDisassembly(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasDisassembly>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.SettingViewCanvasPopup.Close();

            View.OpenSettingButton.onClick.AddListener(() => View.SettingViewCanvasPopup.Open());
            View.OpenSettingButton.transform.DOLocalRotate(new Vector3(0, 0, -360), 12f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(int.MaxValue).SetUpdate(true);
            View.AutoAddButton.onClick.AddListener(AutoAdd);
            View.SaveAutoAddData.onClick.AddListener(SaveAutoAddSetting);
            View.DisassembleButton.OnClick.AddListener(Disassemble);

            View.GoodBackground.DOFade(0, 0.8f).SetLoops(int.MaxValue, LoopType.Yoyo).SetUpdate(true);

            var origin = View.ArrowImage.transform.localPosition;
            View.ArrowImage.transform.DOLocalJump(origin, 20, 1, 1.6f).SetLoops(int.MaxValue, LoopType.Yoyo).SetUpdate(true);
        }

        public void Open(Rune rune)
        {
            if (rune == null) return;
            base.Open();

            Utility.NormalizeGridLayoutSize(View.SlotGridLayoutGroup, View.SlotGridLayoutGroup.constraintCount);
            UpdateRuneList(rune);
            UpdateDynamicValue();
            UpdateCheckBoxes();
        }

        private void Disassemble()
        {
            if (_currRewardValue <= 0)
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.PleaseSelectDisassembleUnit);
                return;
            }

            var selectedCount = 0;
            foreach (var selectedSlot in _viewSlotUis.Where(slot => slot.ActiveButton.IsSelected))
            {
                DataController.Instance.rune.Remove(selectedSlot.Rune);
                selectedCount++;
            }

            DataController.Instance.good.Earn(GoodType.RuneEnhancementStone, _currRewardValue);
            
            ShowReward();
            DataController.Instance.quest.Count(QuestType.DisassembleRune, selectedCount);
            DataController.Instance.disassembly.onBindDisassembly?.Invoke();
            
            Close();
            DataController.Instance.LocalSave();
        }

        private void ShowReward()
        {
            Get<ControllerCanvasToastMessage>()
                .ShowSimpleRewardView(new GoodItem(GoodType.RuneEnhancementStone, _currRewardValue), LocalizeManager.GetText(LocalizedTextType.Claimed));
        }

        private void SaveAutoAddSetting()
        {
            DataController.Instance.disassembly.SaveCheckBoxesOn(View.TypeCheckBoxes, View.GradeCheckBoxes);
            View.SettingViewCanvasPopup.Close();
        }

        private void UpdateRuneList(Rune rune = null)
        {
            var i = 0;
            foreach (var earnedRune in DataController.Instance.rune.runes)
            {
                if(earnedRune.IsEquipped) continue;
                var index = i++;
                
                var slot = GetSlot(index);
                slot
                    .SetUnitSprite(DataController.Instance.rune.GetImage(earnedRune.type, earnedRune.grade))
                    .SetGradeText(earnedRune.grade)
                    .SetActiveEquipBorder( earnedRune.equippedIndex)
                    .SetActiveEnhancementBorder(earnedRune.enhancementLevel > 0)
                    .SetEnhancementLevel(earnedRune.enhancementLevel)
                    .SetActive(true);

                slot.Rune = earnedRune;
                slot.ActiveButton.Selected(false);
                
                if(earnedRune == rune)
                    slot.ActiveButton.Selected(true);
            }

            for (; i < _viewSlotUis.Count; ++i)
            {
                _viewSlotUis[i].SetActive(false);
            }
        }

        private void UpdateDynamicValue()
        {
            _currRewardValue = _viewSlotUis
                .Where(slot => slot.ActiveButton.IsSelected)
                .Select(slot => slot.Rune)
                .Select(rune => DataController.Instance.enhancement.GetDisassembleMaterialValue(rune)).Sum();

            View.MaterialDynamicValue
                .SetGoodType(GoodType.RuneEnhancementStone)
                .ShowValue(_currRewardValue);
        }

        private void UpdateCheckBoxes()
        {
            for (var i = 0; i < View.TypeCheckBoxes.Length; ++i)
            {
                View.TypeCheckBoxes[i].Toggle.isOn = DataController.Instance.disassembly.GetTypeCheckBoxOn(i);
            }
            
            for (var i = 0; i < View.GradeCheckBoxes.Length; ++i)
            {
                View.GradeCheckBoxes[i].Toggle.isOn = DataController.Instance.disassembly.GetGradeCheckBoxIsOn(i);
            }

        }

        private void AutoAdd()
        {
            foreach (var slot in _viewSlotUis.Where(slot => slot.isActiveAndEnabled))
            {
                var rune = slot.Rune;
                var dataDisassembly = DataController.Instance.disassembly;

                slot.ActiveButton.Selected(dataDisassembly.GetGradeCheckBoxIsOn((int)rune.GradeType)
                                           && dataDisassembly.GetTypeCheckBoxOn((int)rune.type));
                UpdateDynamicValue();
            }
        }

        private ViewSlotUI GetSlot(int index)
        {
            if (_viewSlotUis.Count <= index)
            {
                var slot = Object.Instantiate(ResourcesManager.Instance.viewSlotUIPrefab, View.SlotScrollRect.content);
                slot.Button.onClick.AddListener(() =>
                {
                    slot.ActiveButton.Selected(!slot.ActiveButton.IsSelected);
                    UpdateDynamicValue();
                });
                
                _viewSlotUis.Add(slot);
            }
            return _viewSlotUis[index];
        }
    }
}