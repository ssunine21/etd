using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using NUnit.Framework;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasElementalInfo : ControllerCanvas
    {
        private ViewCanvasElementalInfo View => ViewCanvas as ViewCanvasElementalInfo;
        private Elemental _currElemental;
        private readonly ButtonRadioGroup wrapRadioGroup = new();

        private readonly List<ViewSlotAttribute> _equippingAttrSlots = new();
        private readonly List<ViewSlotAttribute> _possessionAttrSlots = new();
        private readonly List<ViewSlotLevelAbility> _viewSlotLevelAbilities = new();
        private readonly List<ViewSlotLevelAbilityIcon> _viewSlotLevelAbilityIcons = new();

        public ControllerCanvasElementalInfo(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasElementalInfo>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.LevelUpButton.OnClick.AddListener(LevelUp);
            View.EnhanceButton.onClick.AddListener(Enhance);
            wrapRadioGroup.AddListener(View.AbilityActiveButton, () => SetActiveWrap(true));
            wrapRadioGroup.AddListener(View.InfoActiveButton,  () => SetActiveWrap(false));
        }

        public ControllerCanvasElementalInfo SetElemental(Elemental elemental)
        {
            _currElemental = elemental;
            return this;
        }
        
        public override void Open()
        {
            base.Open();
            UpdateView();
        }

        private void LevelUp()
        {
            if (DataController.Instance.elemental.TryLevelUp(_currElemental))
            {
                UpdateElementalSlot(_currElemental);
                UpdateAttributeView(_currElemental);
                DataController.Instance.player.UpdateTotalCombat();
            }
        }

        private void SetActiveWrap(bool isAbility)
        {
            View.SetActiveWrap(isAbility);
        }

        private void Enhance()
        {
            var cost = DataController.Instance.enhancement.GetEnhanceCostValue(_currElemental);
            var toastMessage = Get<ControllerCanvasToastMessage>();

            if (DataController.Instance.enhancement.IsMaxLevel(_currElemental))
            {
                toastMessage.ShowTransientToastMessage(LocalizedTextType.EnhanceNoMore);
                return;
            }

            if (DataController.Instance.good.TryConsume(cost.Key, cost.Value))
            {
                if (DataController.Instance.enhancement.TryEnhance(_currElemental))
                {
                    UpdateElementalSlot(_currElemental);
                    UpdateEnhanceButton();
                }
            }
        }
        
        private void UpdateView()
        {
            UpdateElementalSlot(_currElemental);
            UpdateTag(_currElemental);
            UpdateAttributeView(_currElemental);
            UpdateElementalDescriptionView(_currElemental);
            UpdateEnhanceButton();
            UpdateAbilitySlot();

            View.SetAcquired(_currElemental.IsHave);
        }

        private void UpdateElementalSlot(Elemental elemental)
        {
            var enhancementLevel = elemental.enhancementLevel;
            var level = elemental.Level;
            
            View.ViewSlotElemental
                .SetUnitSprite(DataController.Instance.elemental.GetImage(elemental.type, elemental.grade))
                .SetGradeText(elemental.grade)
                .SetActiveEnhancementBorder(enhancementLevel > 0)
                .SetEnhancementLevel(enhancementLevel);
            
            DataController.Instance.elemental.GetExpData(elemental,out var currExp, out var neededNextExp);
            View.ViewLevel
                .SetLevel(level)
                .SetExp(currExp, neededNextExp);

            var canLevelUp = currExp >= neededNextExp;
            View.LevelUpButton.Selected(canLevelUp);
        }
        
        private void UpdateAttributeView(Elemental elemental)
        {
            var equippingSlots = _equippingAttrSlots.GetViewSlots(ViewSlotAttribute.ViewName, View.ViewEquippingAttrsParent, elemental?.equippingAttr.Count ?? 0);
            var possessionSlots = _possessionAttrSlots.GetViewSlots(ViewSlotAttribute.ViewName, View.ViewPossessionAttrsParent, elemental?.possessionAttr.Count ?? 0);
            
            for (var i = 0; i < equippingSlots.Count; ++i)
            {
                var equippingSlot = equippingSlots[i];
                var possessionSlot = possessionSlots[i];
                var equippingAttr = elemental?.equippingAttr[i];
                var possessionAttr = elemental?.possessionAttr[i];

                if (equippingAttr != null)
                {
                    var valueText = equippingAttr.type == AttributeType.RandomTag
                        ? LocalizeManager.GetText((TagType)(int)equippingAttr.value)
                        : $"{equippingAttr.value.ToAttributeValueString(equippingAttr.type)}" +
                          $"<color=orange>(+{DataController.Instance.elemental.GetPowerOfLevelAttrValues(elemental, equippingAttr.type):P1})</color>";

                    valueText = valueText.Replace(" ", "");
                    equippingSlot.SetIndex(equippingAttr.type).SetValueText(valueText).SetActive(true);

                }
                
                if (possessionAttr != null)
                {
                    var valueText = possessionAttr.type == AttributeType.RandomTag
                        ? LocalizeManager.GetText((TagType)(int)possessionAttr.value)
                        : $"{possessionAttr.value.ToAttributeValueString(possessionAttr.type)}" +
                          $"<color=orange>(+{DataController.Instance.elemental.GetPowerOfLevelAttrValues(elemental, possessionAttr.type):P1})</color>";

                    valueText = valueText.Replace(" ", "");
                    possessionSlot.SetIndex(possessionAttr.type).SetValueText(valueText).SetActive(true);
                }
            }
        }
        
        private void UpdateElementalDescriptionView(Elemental elemental)
        {
            for (var i = 0; i < View.ViewSlotElementalInfos.Length; ++i)
            {
                var elementalKey = GetElementalKey(elemental, (EquippedPositionType)i);
                var data = DataController.Instance.elementalCombine.GetElementalCombine(elementalKey);
                
                var description = LocalizeManager.GetElementalDescriptionText(data.descTextType,
                    data.attackSpeed, data.duration, data.attackCountPerSecond, 0, data.attackCoefficient);
                
                View.ViewSlotElementalInfos[i].SetDescText(description);
            }
        }

        private void UpdateTag(Elemental elemental)
        {
            var key = GetElementalKey(elemental, EquippedPositionType.Active);
            var tags = DataController.Instance.elementalCombine.GetTagTyeps(key);
            
            var stringBuilder = new StringBuilder();
            var i = 0;
            foreach (var tag in tags)
            {
                stringBuilder.Append($"#{LocalizeManager.GetText(tag)}");
                stringBuilder.Append(" ");
                ++i;
            }
            View.SetTag(stringBuilder.ToString());
        }

        private void UpdateEnhanceButton()
        {
            var cost = DataController.Instance.enhancement.GetEnhanceCostValue(_currElemental);
            View.EnhanceViewGood
                .SetInit(cost.Key)
                .SetValue(cost.Value);
            
            var probability = DataController.Instance.enhancement.GetProbability(_currElemental);
            View.SetEnhanceProbabilityText(LocalizeManager.GetText(LocalizedTextType.EnhanceProbability, probability));
        }

        private void UpdateAbilitySlot()
        {
            var levelAttr = _currElemental.levelAttr;
            var abilitySlots = _viewSlotLevelAbilities.GetViewSlots(ViewSlotLevelAbility.ViewName, View.ViewSlotLevelAttrParent, levelAttr.Count);

            var elementalLevel = _currElemental.Level;

            var attrTypeCountDict = new Dictionary<AttributeType, int>();
            var attrEnableCountDict = new Dictionary<AttributeType, int>();
            var preTitleText = "<color=orange><b><size=110%>";
            var index0 = 0;
            foreach (var attrs in levelAttr)
            {
                if(attrs.Key <= elementalLevel) attrEnableCountDict[attrs.Value.type] = attrEnableCountDict.GetValueOrDefault(attrs.Value.type, 0) + 1;
                attrTypeCountDict[attrs.Value.type] = attrTypeCountDict.GetValueOrDefault(attrs.Value.type, 0) + 1;
                var count = attrTypeCountDict[attrs.Value.type];
                
                var title = new StringBuilder(preTitleText);
                title.Append(LocalizeManager.GetText(attrs.Value.type)).Append('+', count);
                var desc = $"{LocalizeManager.GetText(attrs.Value.type)} 강화 {count}단계";

                var abilitySlot = abilitySlots[index0];
                abilitySlot.SetLevel(attrs.Key)
                    .SetAbilityTitleText(title.ToString())
                    .SetAbilityContentText(desc)//values[i].ToAttributeValueString(types[i]))
                    .SetLock(attrs.Key > elementalLevel)
                    .SetActive(true);
                index0++;
            }

            var abilityIconSlots = _viewSlotLevelAbilityIcons.GetViewSlots(ViewSlotLevelAbilityIcon.ViewName, View.ViewSlotLevelAttrIconParent, attrTypeCountDict.Count);
            var index1 = 0;
            foreach (var attr in attrTypeCountDict)
            {
                var slot = abilityIconSlots[index1];
                slot.SetIcon(ResourcesManager.AttributeSprites[attr.Key])
                    .SetActiveStar(attr.Value)
                    .OnStar(attrEnableCountDict.GetValueOrDefault(attr.Key, 0))
                    .SetActive(true);
                index1++;
            }
        }
        
        private string GetElementalKey(Elemental elemental, EquippedPositionType equippedPositionType)
        {
            var sb = equippedPositionType switch
            {
                EquippedPositionType.Active => "A",
                EquippedPositionType.Link => "L",
                EquippedPositionType.Passive => "P",
                _ => ""
            } + elemental.type switch
            {
                ElementalType.Fire => "F",
                ElementalType.Water => "I",
                ElementalType.Wind => "W",
                ElementalType.Lighting => "Lg",
                ElementalType.Light => "L",
                ElementalType.Darkness => "D",
                _ => ""
            } + elemental.grade;

            return sb;
        }
    }
}