using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Rune = ETD.Scripts.UserData.DataController.Rune;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasRune : ControllerCanvas
    {
        public int MenuIndex => 1;
        
        private ViewCanvasRune View => ViewCanvas as ViewCanvasRune;
        private ButtonRadioGroup _equippedUnitGroup;
        private ButtonRadioGroup _slotUnitGroup;
        private Sequence _openSequence;
        private Sequence _closeSequence;
        
        private readonly List<ViewSlotUI> _viewSlots = new();
        
        private SlotType _lastSelectedSlotType;
        private int _lastSelectedSlotIndex = -1;
        private int _lastSelectedProjectorIndexForSelectEquipSlot;
        private Rune _currSelectedRune;

        private int _currSelectedProjectorIndex;
        private RectTransform _currSelectedRuneCopyRectTr;
        
        public ControllerCanvasRune(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasRune>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            Init();
            
            DataController.Instance.enhancement.OnBindEnhanced += UpdateEnhancementLevel;
            DataController.Instance.enhancement.OnBindEnhanced += (enhanceable) => UpdateAttributeView(enhanceable as Rune);
            DataController.Instance.enhancement.OnBindEnhanced += (enhanceable) => UpdateDetailView(enhanceable as Rune);
            DataController.Instance.player.OnBindChangedRune += (index) => SetUnitSlotGroup();
            DataController.Instance.player.OnBindChangedElemental += ChangeUnitColor;
            DataController.Instance.disassembly.onBindDisassembly += () =>
            {
                if (DataController.Instance.rune.Gets().Count > 0)
                {
                    _lastSelectedSlotIndex = -1;
                    _slotUnitGroup.Select(0);
                }
                UpdateView();
            };
        }

        private void Init()
        {
            SetEquippedUnitGroup();
            SetUnitSlotGroup();
            
            View.EquippedActiveButton.OnClick.AddListener(() =>
            {
                if (!View.EquippedActiveButton.IsSelected) return;
                if (_equippedUnitGroup.SelectedIndex > 0) return;
                
                SetAllEnableEquippingSlot(true);
            });

            View.OpenDisassemblyViewActiveButton.OnClick.AddListener(() => Get<ControllerCanvasDisassembly>().Open(_currSelectedRune));
            View.OpenEnhancementViewActiveButton.OnClick.AddListener(() =>
            {
                SetAllEnableEquippingSlot(false);
                Get<ControllerCanvasEnhance>().Open(_currSelectedRune);
            });
            
            View.SlideButton.AddListener(SelectProjector);
            
            View.SortSlotActiveButton.OnClick.AddListener(() =>
            {
                View.SortSlotActiveButton.Selected(!View.SortSlotActiveButton.IsSelected);
            });
            
            View.SortSlotActiveButton.onBindChanged += UpdateSlotUnitViews;
            View.CancelEquippingButton.onClick.AddListener(() => SetAllEnableEquippingSlot(false));

            var optionDatas =
                (from RuneType type in Enum.GetValues(typeof(RuneType)) 
                    select new TMP_Dropdown.OptionData(LocalizeManager.GetText(type), DataController.Instance.rune.GetImage(type, GradeType.C), Color.white)).ToList();
            var dropdownEvent = new TMP_Dropdown.DropdownEvent();
            dropdownEvent.AddListener((index) => UpdateSlotUnitViews());
            View.SortDropdown.AddOptions(optionDatas);

            View.SortDropdown.options[0].text = LocalizeManager.GetText(LocalizedTextType.SeeAll);
            View.SortDropdown.onValueChanged = dropdownEvent;
            
            RotateIcon();
        }

        public override void Open()
        {
            base.Open();
            
            _lastSelectedSlotIndex = -1;
            NormalizeGridLayoutSize(View.GridLayoutGroup, View.GridLayoutGroup.constraintCount);
            UpdateView();
        
            View.ProjectorUI.PlayStartAnimation();
        
            var slot = _viewSlots.Count > 0 ? GetSlot(0) : null;
            if(slot) slot.Button.onClick.Invoke();
            
            SetAllEnableEquippingSlot(false);
        }

        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.EquipRune => GetRuneEqulsProjectorTag(_currSelectedProjectorIndex).gameObject,
                TutorialType.EquipRuneSlot => View.EquippedUnitSlots[0].gameObject,
                TutorialType.Tag => View.TagTextView,
                _ => null
            };
        }
        
        public RectTransform GetGuideArrowParent(QuestType questType)
        {
            return questType switch
            {
                QuestType.EnhanceRune => View.EnhanceGuideArrowRectTransform,
                QuestType.DisassembleRune => View.DisassembleGuideArrowRectTransform,
                _ => View.EnhanceGuideArrowRectTransform
            };
        }

        private ViewSlotUI GetRuneEqulsProjectorTag(int projectorIndex)
        {
            var tags = DataController.Instance.attribute.GetHasTags(projectorIndex);
            ViewSlotUI randomTagSlot = null;
            ViewSlotUI firstSlot = null;
            
            foreach (var slot in _viewSlots)
            {
                if (!slot.Rune.IsEquipped)
                {
                    var runeType = slot.Rune.type;
                    foreach (var tag in tags)
                    {
                        if((runeType == RuneType.Projectile && tag == TagType.Projectile) 
                           || (runeType == RuneType.Chain && tag == TagType.Chain) 
                           || (runeType == RuneType.Duration && tag == TagType.Duration) 
                           || (runeType == RuneType.Expansion && tag == TagType.Expansion))
                        {
                            return slot;
                        }
                    }

                    if (randomTagSlot == null && runeType == RuneType.RandomTag)
                        randomTagSlot = slot;
                    if (firstSlot == null)
                        firstSlot = slot;
                }
            }

            return randomTagSlot != null ? randomTagSlot : firstSlot != null ? firstSlot : null;
        }

        private void SelectProjector(int selectedIndex)
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, 2);
            if (_currSelectedProjectorIndex == selectedIndex) return;
            _currSelectedProjectorIndex = selectedIndex;
            
            UpdateView();
            
            SetAllEnableEquippingSlot(false);
            OnClickPrevSelectedSlot();
        }
        
        private void NormalizeGridLayoutSize(GridLayoutGroup gridLayoutGroup, int constraintCount)
        {
            if (gridLayoutGroup.TryGetComponent<RectTransform>(out var rectTransform))
            {
                var width = rectTransform.rect.width - gridLayoutGroup.spacing.x * (constraintCount + 1);
                var cellSize = width / constraintCount;
                gridLayoutGroup.cellSize = Vector2.one * cellSize;
            }
        }

        private void OnEquippingAnimation(bool flag)
        {
            var duration = 0.25f;
            var elementalMoveDuration = duration - 0.1f;
            var newHeight = flag ? 680f : 580;
            var newOffsetMax = new Vector2(-0, -newHeight);
            
            if (flag)
            {
                View.ShadowPanel.SetActive(true);
                View.EquippingAnimCanvasGroup.DOFade(1f, duration).SetUpdate(true);

                if (_currSelectedRuneCopyRectTr)
                {
                    Object.DestroyImmediate(_currSelectedRuneCopyRectTr.gameObject);
                }

                _currSelectedRuneCopyRectTr =
                    Object.Instantiate(GetSlot(_slotUnitGroup.SelectedIndex).GetComponent<RectTransform>(),
                        View.ShadowPanel.transform);

                _currSelectedRuneCopyRectTr.sizeDelta = View.GridLayoutGroup.cellSize;
                _currSelectedRuneCopyRectTr.DOLocalMove(new Vector2(0, 100), elementalMoveDuration).SetUpdate(true);
            }
            else
            {
                if (_currSelectedRuneCopyRectTr)
                {
                    Object.DestroyImmediate(_currSelectedRuneCopyRectTr.gameObject);
                }
                
                View.EquippingAnimCanvasGroup
                    .DOFade(0f, duration).OnComplete(() => View.ShadowPanel.SetActive(false)).SetUpdate(true);
            }

            View.MainViewRect
                .DOSizeDelta(new Vector2(View.MainViewRect.sizeDelta.x, newHeight), duration).SetUpdate(true);
            DOTween.To(() => View.SubViewRect.offsetMax, x => View.SubViewRect.offsetMax = x, newOffsetMax, duration);
        }
        
        private void UpdateEquippedSlotViews()
        {
            var selectedIndex = 0;
            foreach (var rune in DataController.Instance.player.GetEquippedRunes(_currSelectedProjectorIndex))
            {
                var isLock = DataController.Instance.upgrade.IsLockRune(_currSelectedProjectorIndex, selectedIndex);
                var slot = View.EquippedUnitSlots[selectedIndex];

                slot.SetLock(isLock);
                if (!slot) continue;
                if (rune == null)
                {
                    slot.SetInit();
                    selectedIndex++;
                    continue;
                }
                slot
                    .SetUnitSprite(DataController.Instance.rune.GetImage(rune.type, rune.grade))
                    .SetGradeText(rune.grade)
                    .SetActiveEquipBorder(rune.equippedIndex)
                    .SetActiveEnhancementBorder(rune.enhancementLevel > 0)
                    .SetEnhancementLevel(rune.enhancementLevel);

                slot.Rune = rune;

                selectedIndex++;
            }
        }

        private void UpdateAttributeView(Rune rune)
        {
            SetAttributeView(View.ViewEquippingAttrs, rune);
        }

        private void UpdateDetailView(Rune rune)
        {
            if (rune == null) return;
            
            //_view.EquippedActiveButton.Selected(!rune.IsEquipped);
            View.OpenEnhancementViewActiveButton.Selected(!DataController.Instance.enhancement.IsMaxLevel(rune));
            View.OpenDisassemblyViewActiveButton.Selected(!rune.IsEquipped);
        }

        private void UpdateProjectorView()
        {
            View.ProjectorUI.SetActiveUnit(_currSelectedProjectorIndex);
            View.LockPanel.SetActive(_currSelectedProjectorIndex > DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseProjector));
        }
        
        private void UpdateEnhancementLevel(IEnhanceable rune)
        {
            var slot = _viewSlots.Find((x) => x.Rune == rune);
            if (slot != null)
            {                
                slot
                    .SetActiveEnhancementBorder(rune.EnhancementLevel > 0)
                    .SetEnhancementLevel(rune.EnhancementLevel);
            }

            var equippedSlot = View.EquippedUnitSlots.ToList().Find((x) => x.Rune == rune);
            if (equippedSlot != null)
            {
                equippedSlot
                    .SetActiveEnhancementBorder(rune.EnhancementLevel > 0)
                    .SetEnhancementLevel(rune.EnhancementLevel);
            }
        }

        private void UpdateSlotUnitViews()
        {
            var runes = DataController.Instance.rune.Gets();
            var isRuneEmpty = runes.Count == 0;
            View.EmptyTextPanel.SetActive(isRuneEmpty);
            if (isRuneEmpty) return;

            var runesOrderByGrade = View.SortSlotActiveButton.IsSelected
                ? runes.OrderByDescending(rune => rune.grade)
                : runes.OrderBy(rune => rune.grade);

            var conditionRune = View.SortDropdown.value;
            
            var i = 0;
            foreach (var rune in runesOrderByGrade)
            {
                if (conditionRune > 0)
                {
                    var runeType = (RuneType)(conditionRune - 1);
                    if (rune.type != runeType) continue;
                }

                var index = i;
                var slot = GetSlot(index);

                slot
                    .SetUnitSprite(DataController.Instance.rune.GetImage(rune.type, rune.grade))
                    .SetGradeText(rune.grade)
                    .SetActiveEquipBorder(rune.equippedIndex)
                    .SetActiveEnhancementBorder(rune.enhancementLevel > 0)
                    .SetEnhancementLevel(rune.enhancementLevel)
                    .SetActive(true);

                slot.Rune = rune;
                ++i;
            }

            View.EmptyTextPanel.SetActive(i == 0);

            for (; i < _viewSlots.Count; ++i)
            {
                _viewSlots[i].SetActive(false);
            }
        }

        private void UpdateView()
        {
            View.CountText.text = 
                $"{DataController.Instance.rune.runes.Count}/{DataController.Instance.rune.MaxRuneCount}";
            View.CountText.color = DataController.Instance.rune.IsMaxRune ? Color.red : Color.white;
            
            UpdateEquippedSlotViews();
            UpdateSlotUnitViews();
            UpdateProjectorView();
            UpdateTag();
            ChangeUnitColor(_currSelectedProjectorIndex);
        }
        
        private void UpdateTag()
        {
            var tags = DataController.Instance.attribute.GetHasTags(_currSelectedProjectorIndex);
            var stringBuilder = new StringBuilder();
            
            var i = 0;
            foreach (var tag in tags)
            {
                if (i > 0 && i % 4 == 0)
                {
                    stringBuilder.Append("\n");
                }
                
                var attributeType = Utility.TagTypeToAttributeType(tag);
                var valueString = 
                    DataController.Instance.attribute.GetValue(attributeType, _currSelectedProjectorIndex)
                        .ToAttributeValueString(attributeType);

                stringBuilder.Append($"#{LocalizeManager.GetText(tag)}");

                if (!string.IsNullOrEmpty(valueString))
                    stringBuilder.Append($"<color=orange>({valueString})</color>");
                stringBuilder.Append(" ");
                ++i;
            }
            View.SetTagText(stringBuilder.ToString());
        }

        private ViewSlotUI GetSlot(int index)
        {
            _slotUnitGroup ??= new ButtonRadioGroup();
            if (_viewSlots.Count > index) return _viewSlots[index];

            var unit = Object.Instantiate(ResourcesManager.Instance.viewSlotUIPrefab, View.GridLayoutGroup.transform);
            unit.name += index;
            
            _viewSlots.Add(unit);
            return _viewSlots[index];
        }

        private void SetAllEnableEquippingSlot(bool flag)
        {
            foreach (var equippedUnitSlot in View.EquippedUnitSlots)
            {
                equippedUnitSlot.SetEnableEquipping(!equippedUnitSlot.IsLock && flag);
            }

            OnEquippingAnimation(flag);
        }

        private void SetAttributeView(IReadOnlyList<ViewSlotAttribute> viewAttributes, Rune rune)
        {
            var i = 0;
            if(rune != null)
            {
                foreach (var attribute in rune.equippingAttr)
                {
                    viewAttributes[i]
                        .SetIndex(attribute.type)
                        .SetValue(attribute.type, attribute.value)
                        .SetActive(true);

                    ++i;
                }
            }

            for (; i < viewAttributes.Count; ++i)
                viewAttributes[i].SetActive(false);
        }

        private void SetEquippedUnitGroup()
        {
            _equippedUnitGroup ??= new ButtonRadioGroup();
            if (_equippedUnitGroup.Count == 0)
            {
                var i = 0;
                foreach (var slot in View.EquippedUnitSlots)
                {
                    var index = i;
                    _equippedUnitGroup.AddListener(slot.ActiveButton,
                        () =>
                        {
                            var toastMessage = Get<ControllerCanvasToastMessage>();
                            if(slot.IsLock)
                            {
                                toastMessage.ShowTransientToastMessage(LocalizedTextType.SlotIsLock);
                                OnClickPrevSelectedSlot();
                            }
                            else if (slot.Rune == null && !slot.IsEnableEquipping)
                            {
                                toastMessage.ShowTransientToastMessage(LocalizedTextType.EmptySlotMessage);
                                OnClickPrevSelectedSlot();
                            }
                            else
                            {
                                if (slot.IsEnableEquipping)
                                {
                                    DataController.Instance.player.EquipRune(_currSelectedProjectorIndex,
                                        (EquippedPositionType)index, _currSelectedRune);
                                    slot.Rune = _currSelectedRune;
                                    
                                    DataController.Instance.player.UpdateTotalCombat();
                                }

                                _currSelectedRune = slot.Rune;
                                
                                if (_currSelectedRune != null)
                                {
                                    if (TryEquipUnit(SlotType.EquipSlot, index))
                                    {
                                        UpdateSlotUnitViews();
                                    }
                                }

                                if (_currSelectedRune != null)
                                    SetEquipText(slot, _currSelectedRune.IsEquipped);
                                UpdateAttributeView(slot.Rune);
                                UpdateDetailView(slot.Rune);
                                UpdateView();
                                
                                SetAllEnableEquippingSlot(false);

                                OnSelectSlot(SlotType.EquipSlot, index);
                            }
                        });
                    ++i;
                }
            }
        }

        private void SetUnitSlotGroup()
        {
            for (var i = _viewSlots.Count; i < DataController.Instance.rune.Gets()?.Count; ++i)
            {
                var slot = GetSlot(i);
                var index = i;
                _slotUnitGroup.AddListener(slot.ActiveButton, () =>
                {
                    _currSelectedRune = slot.Rune;
                    
                    if( TryEquipUnit(SlotType.UnitSlot, index))
                    {
                        UpdateSlotUnitViews();
                    }

                    if (_currSelectedRune != null)
                        SetEquipText(slot, _currSelectedRune.IsEquipped);
                    UpdateAttributeView(_currSelectedRune);
                    UpdateDetailView(_currSelectedRune);
                    UpdateEquippedSlotViews();
                    
                    OnSelectSlot(SlotType.UnitSlot, index);
                });
            }
        }
        
        private bool TryEquipUnit(SlotType slotType, int index)
        {
            if (_lastSelectedSlotType == slotType
                && _lastSelectedSlotIndex == index)
            {
                if(_currSelectedRune.IsEquipped)
                {
                    DataController.Instance.player.UnEquipRune(_currSelectedRune.equippedIndex,
                        _currSelectedRune.equippedPositionType);
                    
                    _equippedUnitGroup.Select(-1);
                    DataController.Instance.player.UpdateTotalCombat();
                }
                else 
                    SetAllEnableEquippingSlot(_lastSelectedSlotIndex == index);
                return true;
            }

            return false;
        }

        private void SetEquipText(ViewSlotUI viewSlotRune, bool isEquipped)
        {
            var text = isEquipped
                ? LocalizeManager.GetText(LocalizedTextType.UnEquipText)
                : LocalizeManager.GetText(LocalizedTextType.EquipText);
            viewSlotRune.SetEquippedText(text);
        }
        
        private void OnSelectSlot(SlotType type, int index)
        {
            _lastSelectedSlotType = type;
            _lastSelectedSlotIndex = index;
            _lastSelectedProjectorIndexForSelectEquipSlot = _currSelectedProjectorIndex;
            
            if(type == SlotType.EquipSlot)
                _slotUnitGroup.Select(-1);
            else
                _equippedUnitGroup.Select(-1);
        }
        
        private void RotateIcon()
        {
            var endValue = new Vector3(0, 0, -180);
            View.RoundArrow.DORotate(endValue, 1f).SetEase(Ease.OutCubic).SetLoops(int.MaxValue).SetUpdate(true);
        }
        
        private void ChangeUnitColor(int projectorIndex)
        {
            var color = DataController.Instance.player.GetProjectorUnitColor(projectorIndex);
            View.ProjectorUI.ChangeColor(color);
        }

        private void OnClickPrevSelectedSlot()
        {
            var selectedIndex = _lastSelectedSlotIndex;
            _lastSelectedSlotIndex = -1;
            
            if(_lastSelectedSlotType == SlotType.UnitSlot)
            {
                if (_slotUnitGroup is { Count: > 0 })
                    _slotUnitGroup.Select(selectedIndex);
            }
            else
            {
                if(_equippedUnitGroup is {Count: > 0})
                {
                    if (_lastSelectedProjectorIndexForSelectEquipSlot != _currSelectedProjectorIndex)
                        _equippedUnitGroup.Select(-1);
                    else
                        _equippedUnitGroup.Select(selectedIndex);
                }
            }
        }
    }
}