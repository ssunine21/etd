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
using JetBrains.Annotations;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasElemental : ControllerCanvas
    {
        public int MenuIndex => 0;

        private ViewCanvasElemental View => ViewCanvas as ViewCanvasElemental;
        private ButtonRadioGroup _equippedUnitGroup = new();
        private ButtonRadioGroup _slotUnitGroup =  new();
        private ButtonRadioGroup _slotsSortRadioGroup = new();
        private Sequence _openSequence;
        private Sequence _closeSequence;
        
        private readonly List<ViewSlotElemental> _viewSlots = new();

        private int _currSelectedProjectorIndex;

        private SlotType _lastSelectedSlotType;
        private int _lastSelectedSlotIndex = -1;
        private int _lastSelectedProjectorIndexForSelectEquipSlot;

        private Elemental _currSelectedElemental;
        private RectTransform _currSelectedElementalCopyRectTr;
        private RectTransform _viewRectTr;
        
        public ControllerCanvasElemental(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasElemental>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            Init();

            DataController.Instance.enhancement.OnBindEnhanced += (enhanceable) =>
            {
                UpdateEnhancementLevel(enhanceable);
                UpdateAttributeView(enhanceable as Elemental);
                UpdateDetailView(enhanceable as Elemental);
            };

            DataController.Instance.elemental.OnBindLevelUp += (elemental) =>
            {
                UpdateDetailView(elemental);
                UpdateSlotEach(elemental);
                UpdateAttributeView(elemental);
            };
            
            DataController.Instance.elemental.OnBindEarn += UpdateDetailView;
            DataController.Instance.elemental.OnBindEarn += UpdateSlotEach;
            DataController.Instance.player.OnBindChangedElemental += ChangeUnitColor;
            
            View.LevelUpEffectImage
                .DOFade(0, 0.7f)
                .SetLoops(int.MaxValue, LoopType.Yoyo)
                .SetUpdate(true);
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
            
            View.OpenEnhancementViewActiveButton.OnClick.AddListener(() =>
            {
                SetAllEnableEquippingSlot(false);
                Get<ControllerCanvasEnhance>().Open(_currSelectedElemental);
            });
            
            View.AllLevelUpButton.onClick.AddListener(() =>
            {
                foreach (var slot in _viewSlots)
                {
                    DataController.Instance.elemental.AllLevelUp(slot.Elemental);
                }
                DataController.Instance.player.UpdateTotalCombat();
            });
            
            View.SlideButton.AddListener(SelectProjector);

            _slotsSortRadioGroup = new ButtonRadioGroup();
            
            foreach (var activeButton in View.ChangeContentsViewActiveButtons)
            {
                _slotsSortRadioGroup.AddListener(activeButton, UpdateSlotUnitViews);
            }
            _slotsSortRadioGroup.Select(0);

            View.SortSlotActiveButton.OnClick.AddListener(() => View.SortSlotActiveButton.Selected(!View.SortSlotActiveButton.IsSelected));
            View.SortSlotActiveButton.onBindChanged += UpdateSlotUnitViews;
            View.CancelEquippingButton.onClick.AddListener(() => SetAllEnableEquippingSlot(false));
            View.ShowInfoButon.onClick.AddListener(() =>
            {
                Get<ControllerCanvasElementalInfo>().SetElemental(_currSelectedElemental).Open();
            });

            if (View.WrapCanvasGroup.TryGetComponent<RectTransform>(out var rect))
            {
                _viewRectTr = rect;
            }
            
            RotateIcon();
        }

        public override void Open()
        {
            base.Open();

            _lastSelectedSlotIndex = -1;
            NormalizeGridLayoutSize();
        
            UpdateView();
        
            View.ProjectorUI.PlayStartAnimation();
        
            var slot = GetSlot(0);
            if (slot)
                slot.Button.onClick.Invoke();
            SetAllEnableEquippingSlot(false);
        }
        
        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.DescriptionActiveSlot => View.EquippedUnitSlots[0].gameObject,
                TutorialType.DescriptionLinkSlot => View.EquippedUnitSlots[1].gameObject,
                TutorialType.DescriptionPassiveSlot => View.EquippedUnitSlots[2].gameObject,
                TutorialType.SelectElementalSlot => GetUnEquippedSlot()?.gameObject,
                TutorialType.EquipElementalButton => View.EquippedActiveButton.gameObject,
                TutorialType.EquipLinkSlot => View.EquippedUnitSlots[1].gameObject,
                TutorialType.Tag => View.TagTextView,
                _ => null
            };
        }

        private ViewSlotElemental GetUnEquippedSlot()
        {
            return _viewSlots.Find(x => !x.Elemental.IsEquipped && x.Elemental.IsHave);
        }

        private void ChangeUnitColor(int projectorIndex)
        {
            var color = DataController.Instance.player.GetProjectorUnitColor(projectorIndex);
            View.ProjectorUI.ChangeColor(color);
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
                            if (slot.IsLock)
                            {
                                toastMessage.ShowTransientToastMessage(LocalizedTextType.SlotIsLock);
                                OnClickPrevSelectedSlot();
                            }
                            else if (slot.Elemental == null && !slot.IsEnableEquipping)
                            {
                                toastMessage.ShowTransientToastMessage(LocalizedTextType.EmptySlotMessage);
                                OnClickPrevSelectedSlot();
                            }
                            else
                            {
                                if (slot.IsEnableEquipping)
                                {
                                    DataController.Instance.player.EquipElemental(_currSelectedProjectorIndex,
                                        (EquippedPositionType)index, _currSelectedElemental);
                                    slot.Elemental = _currSelectedElemental;
                                    DataController.Instance.player.UpdateTotalCombat();
                                }

                                _currSelectedElemental = slot.Elemental;
                                
                                if (_currSelectedElemental != null)
                                {
                                    if( TryEquipUnit(SlotType.EquipSlot, index))
                                    {
                                        UpdateSlotUnitViews();
                                    }
                                }

                                if (_currSelectedElemental != null)
                                    SetEquipText(slot, _currSelectedElemental.IsEquipped);
                                UpdateAttributeView(slot.Elemental);
                                UpdateDetailView(slot.Elemental);
                                UpdateView();
                                
                                SetAllEnableEquippingSlot(false);

                                OnSelectSlot(SlotType.EquipSlot, index);
                            }
                        });
                    i++;
                }
            }
        }

        private void SetUnitSlotGroup()
        {
            for(var i = 0; i < DataController.Instance.elemental.Gets().Count; ++i)
            {
                var slot = GetSlot(i);
                var index = i;
                _slotUnitGroup.AddListener(slot.ActiveButton, () =>
                {              
                    _currSelectedElemental = slot.Elemental;
                    if( TryEquipUnit(SlotType.UnitSlot, index))
                    {
                        UpdateSlotUnitViews();
                    }

                    if (_currSelectedElemental != null)
                        SetEquipText(slot, _currSelectedElemental.IsEquipped);
                    
                    UpdateAttributeView(_currSelectedElemental);
                    UpdateDetailView(_currSelectedElemental);
                    UpdateEquippedSlotViews();

                    if (slot.TryGetComponent<RectTransform>(out var component))
                    {
                        var corners = new Vector3[4];
                        component.GetWorldCorners(corners);
                        
                        if (View.ShowInfoButon.TryGetComponent<RectTransform>(out var buttonComponent))
                        {
                            var centerBottom = (corners[0] + corners[3]) * 0.5f;
                            centerBottom.y -= buttonComponent.sizeDelta.y * 0.006f;
                            buttonComponent.sizeDelta = new Vector2(component.sizeDelta.x, buttonComponent.sizeDelta.y);
                            buttonComponent.position = centerBottom;
                            
                            buttonComponent.transform.SetAsLastSibling();
                        }
                    }
                    
                    OnSelectSlot(SlotType.UnitSlot, index);
                });
            }
        }

        private bool TryEquipUnit(SlotType slotType, int index)
        {
            if (_lastSelectedSlotType == slotType && _lastSelectedSlotIndex == index)
            {
                if (!_currSelectedElemental.IsHave) return false;
                if(_currSelectedElemental.IsEquipped)
                {
                    DataController.Instance.player.UnEquipElemental(_currSelectedElemental.equippedIndex,
                        _currSelectedElemental.equippedPositionType);
                    
                    DataController.Instance.player.UpdateTotalCombat();
                    _equippedUnitGroup.Select(-1);
                }
                else 
                    SetAllEnableEquippingSlot(_lastSelectedSlotIndex == index);
                return true;
            }

            return false;
        }

        private void SetEquipText(ViewSlotUI viewSlotElemental, bool isEquipped)
        {
            var text = isEquipped
                ? LocalizeManager.GetText(LocalizedTextType.UnEquipText)
                : LocalizeManager.GetText(LocalizedTextType.EquipText);
            viewSlotElemental.SetEquippedText(text);
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

        private void OnClickPrevSelectedSlot()
        {
            var selectedIndex = _lastSelectedSlotIndex;
            _lastSelectedSlotIndex = -1;
            
            if (_lastSelectedSlotType == SlotType.UnitSlot)
                _slotUnitGroup.Select(selectedIndex);
            else if (_lastSelectedSlotType == SlotType.EquipSlot)
            {
                if (_lastSelectedProjectorIndexForSelectEquipSlot != _currSelectedProjectorIndex)
                    _equippedUnitGroup.Select(-1);
                else
                    _equippedUnitGroup.Select(selectedIndex);
            }
        }
        
        private void NormalizeGridLayoutSize()
        {
            if (!View.GridLayoutGroup.TryGetComponent<RectTransform>(out var rectTransform)) return;
            var width = rectTransform.rect.width - (View.GridLayoutGroup.spacing.x * 7);
            var cellSize = width / 6;
            View.GridLayoutGroup.cellSize = new Vector2(cellSize, cellSize * 1.11f);
        }

        private void OnEquippingAnimation(bool flag)
        {
            var duration = 0.25f;
            var elementalMoveDuration = duration - 0.1f;
            var newHeight = flag ? 700f : 600f;
            var newOffsetMax = new Vector2(-0, -newHeight);
            
            if (flag)
            {
                View.ShadowPanel.SetActive(true);
                View.EquippingAnimCanvasGroup
                    .DOFade(1f, duration)
                    .SetUpdate(true);

                if (_currSelectedElementalCopyRectTr)
                {
                    Object.DestroyImmediate(_currSelectedElementalCopyRectTr.gameObject);
                }
                
                _currSelectedElementalCopyRectTr = Object.Instantiate(GetSlot(_slotUnitGroup.SelectedIndex).GetComponent<RectTransform>(), View.ShadowPanel.transform);
                _currSelectedElementalCopyRectTr.sizeDelta = View.GridLayoutGroup.cellSize;
                _currSelectedElementalCopyRectTr
                    .DOLocalMove(new Vector2(0, 100), elementalMoveDuration)
                    .SetUpdate(true);
            }
            else
            {
                if (_currSelectedElementalCopyRectTr)
                {
                    Object.DestroyImmediate(_currSelectedElementalCopyRectTr.gameObject);
                }
                
                View.EquippingAnimCanvasGroup
                    .DOFade(0f, duration).OnComplete(() => View.ShadowPanel.SetActive(false))
                    .SetUpdate(true);
            }

            View.MainViewRect
                .DOSizeDelta(new Vector2(View.MainViewRect.sizeDelta.x, newHeight), duration)
                .SetUpdate(true);
            DOTween
                .To(() => View.SubViewRect.offsetMax, x => View.SubViewRect.offsetMax = x, newOffsetMax, duration)
                .SetUpdate(true);
        }

        private void UpdateView()
        {
            UpdateEquippedSlotViews();
            UpdateSlotUnitViews();
            UpdateProjectorView();
            UpdateTag();
            ChangeUnitColor(_currSelectedProjectorIndex);
        }

        private void UpdateEquippedSlotViews()
        {
            var selectedIndex = 0;
            foreach (var elemental in DataController.Instance.player.GetEquippedElementals(_currSelectedProjectorIndex))
            {
                var isLock =
                    selectedIndex == 0 
                        ? _currSelectedProjectorIndex > DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseProjector)
                        : DataController.Instance.upgrade.IsLockElemental(_currSelectedProjectorIndex, selectedIndex);
                
                var slot = View.EquippedUnitSlots[selectedIndex];
                slot.SetLock(isLock);
                
                if (!slot) continue;
                if (elemental == null)
                {
                    slot.SetInit();
                    selectedIndex++;
                    continue;
                }
                
                slot
                    .SetUnitSprite(DataController.Instance.elemental.GetImage(elemental.type, elemental.grade))
                    .SetGradeText(elemental.grade)
                    .SetActiveEquipBorder(elemental.equippedIndex)
                    .SetActiveEnhancementBorder(elemental.enhancementLevel > 0)
                    .SetEnhancementLevel(elemental.enhancementLevel);

                slot.ViewLevel
                    .SetActive(true)
                    .SetLevel(elemental.Level);

                slot.Elemental = elemental;
                selectedIndex++;
            }
        }
        
        private void UpdateProjectorView()
        {
            View.ProjectorUI.SetActiveUnit(_currSelectedProjectorIndex);
            View.LockPanel.SetActive(_currSelectedProjectorIndex > DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseProjector));
        }

        private void UpdateEnhancementLevel(IEnhanceable elemental)
        {
            var slot = _viewSlots.Find((x) => x.Elemental == elemental);
            if (slot != null)
            {                
                slot
                    .SetActiveEnhancementBorder(elemental.EnhancementLevel > 0)
                    .SetEnhancementLevel(elemental.EnhancementLevel);
            }

            var equippedSlot = View.EquippedUnitSlots.ToList().Find((x) => x.Elemental == elemental);
            if (equippedSlot != null)
            {
                equippedSlot
                    .SetActiveEnhancementBorder(elemental.EnhancementLevel > 0)
                    .SetEnhancementLevel(elemental.EnhancementLevel);
            }
        }

        private void UpdateSlotEach(Elemental elemental)
        {
            DataController.Instance.elemental.GetExpData(elemental,out var currExp, out var neededNextExp);
            var slot = _viewSlots.Find((x) => x.Elemental == elemental);
            if (slot)
            {
                slot.ViewLevel
                    .SetLevel(elemental.Level)
                    .SetExp(currExp, neededNextExp);
                
                slot.ReddotView.ShowReddot(currExp >= neededNextExp && elemental.IsHave);
            }

            var equippedSlot = View.EquippedUnitSlots.ToList().Find((x) => x.Elemental == elemental);
            if (equippedSlot)
            {
                equippedSlot.ViewLevel.SetLevel(elemental.Level);
            }
            
        }

        private void UpdateSlotUnitViews()
        {
            var elementals = DataController.Instance.elemental.Gets();
            if (elementals == null) return;

            var elementalOrderByGrade = View.SortSlotActiveButton.IsSelected
                ? elementals.OrderByDescending(elemental => elemental.grade)
                : elementals.OrderBy(elemental => elemental.grade);
            
            var i = 0;
            foreach (var userElemental in elementalOrderByGrade)
            {
                var index = i;
                var slot = GetSlot(index);
                var isActive = userElemental.IsHave || _slotsSortRadioGroup.SelectedIndex == 0;

                slot
                    .SetUnitSprite(DataController.Instance.elemental.GetImage(userElemental.type, userElemental.grade))
                    .SetGradeText(userElemental.grade)
                    .SetActiveEquipBorder(userElemental.equippedIndex)
                    .SetActiveEnhancementBorder(userElemental.enhancementLevel > 0)
                    .SetEnhancementLevel(userElemental.enhancementLevel)
                    .SetActive(isActive);

                slot.SetLock(!userElemental.IsHave);
                
                DataController.Instance.elemental.GetExpData(userElemental,out var currExp, out var neededNextExp);
                slot.ViewLevel
                    .SetLevel(userElemental.Level)
                    .SetExp(currExp, neededNextExp);
                
                slot.ReddotView.ShowReddot(currExp >= neededNextExp && userElemental.IsHave);

                slot.Elemental = userElemental;
                ++i;
            }

            for (; i < _viewSlots.Count; ++i)
            {
                _viewSlots[i].SetActive(false);
            }
        }

        private void UpdateDetailView(Elemental elemental) 
        {
            if(elemental == null) return;
            
            if (!elemental.IsHave)
            {
                View.SetLock(true);
            }
            else
            {
                View.SetLock(false);
                DataController.Instance.elemental.GetExpData(elemental, out var currExp, out var neededNextExp);

                
                View.ViewLevel
                    .SetLevel(elemental.Level)
                    .SetExp(currExp, neededNextExp);
            }
        }

        private void UpdateAttributeView([CanBeNull] Elemental elemental)
        {
            SetAttributeView(View.ViewEquippingAttrs, elemental, elemental?.equippingAttr);
            SetAttributeView(View.ViewPossessionAttrs, elemental, elemental?.possessionAttr);
        }

        private void UpdateTag()
        {
            var tags = DataController.Instance.attribute.GetHasTags(_currSelectedProjectorIndex);
            var stringBuilder = new StringBuilder();

            var i = 0;
            foreach (var tag in tags)
            {
                if (i > 0 && i % 3 == 0)
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

        private void SetAllEnableEquippingSlot(bool flag)
        {
            foreach (var equippedUnitSlot in View.EquippedUnitSlots)
            {
                equippedUnitSlot.SetEnableEquipping(!equippedUnitSlot.IsLock && flag);
            }
            
            OnEquippingAnimation(flag);
        }

        private void SetAttributeView(IReadOnlyList<ViewSlotAttribute> viewAttributes, Elemental elemental, [CanBeNull] List<Attribute> attributes)
        {
            var i = 0;
            if (attributes != null)
                foreach (var attribute in attributes)
                {
                    var valueText = attribute.type == AttributeType.RandomTag
                        ? LocalizeManager.GetText((TagType)(int)attribute.value)
                        : $"{attribute.value.ToAttributeValueString(attribute.type)}" +
                          $"<color=orange>(+{DataController.Instance.elemental.GetPowerOfLevelAttrValues(elemental, attribute.type):P1})</color>";

                    valueText = valueText.Replace(" ", "");
                    viewAttributes[i]
                        .SetIndex(attribute.type)
                        .SetValueText(valueText)
                        .SetActive(true);

                    ++i;
                }

            for (; i < viewAttributes.Count; ++i)
                viewAttributes[i].SetActive(false);
        }
        
        private void RotateIcon()
        {
            var endValue = new Vector3(0, 0, -180);
            View.RoundArrow
                .DORotate(endValue, 1f).SetEase(Ease.OutCubic).SetLoops(int.MaxValue).SetUpdate(true);
        }

        private ViewSlotElemental GetSlot(int index)
        {
            _slotUnitGroup ??= new ButtonRadioGroup();
            if (_viewSlots.Count > index) return _viewSlots[index];
            var unit = Object.Instantiate(ResourcesManager.Instance.viewSlotElementalPrefab, View.GridLayoutGroup.transform);
            _viewSlots.Add(unit);
            return _viewSlots[index];
        }
    }
}