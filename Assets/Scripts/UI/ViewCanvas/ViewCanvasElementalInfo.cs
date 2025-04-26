using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasElementalInfo : ViewCanvas
    {
        public ViewSlotElemental ViewSlotElemental => viewSlotElemental;
        public ViewLevel ViewLevel => viewLevel;
        public Transform ViewEquippingAttrsParent => viewEquippingAttrsParent;
        public Transform ViewPossessionAttrsParent => viewPossessionAttrsParent;
        public ActiveButton LevelUpButton => levelUpButton;
        public Button EnhanceButton => enhanceButton;
        public ViewGood EnhanceViewGood => enhanceViewGood;
        public ViewSlotElementalInfo[] ViewSlotElementalInfos => viewSlotElementalInfos;
        public ActiveButton AbilityActiveButton => abilityActiveButton;
        public ActiveButton InfoActiveButton => infoActiveButton;
        public Transform ViewSlotLevelAttrParent => viewSlotLevelAttrParent;
        public Transform ViewSlotLevelAttrIconParent => viewSlotLevelAttrIconParent;
        
        [Space] [Space]
        [SerializeField] private GameObject abilityGo;
        [SerializeField] private GameObject infoGo;
        [SerializeField] private ActiveButton abilityActiveButton;
        [SerializeField] private ActiveButton infoActiveButton;

        
        [SerializeField] private ViewSlotElemental viewSlotElemental;
        [SerializeField] private ViewLevel viewLevel;
        [SerializeField] private TMP_Text tagTMP;
        
        [Space] [Space]
        [SerializeField] private Transform viewEquippingAttrsParent;
        [SerializeField] private Transform viewPossessionAttrsParent;

        [Space] [Space]
        [SerializeField] private Transform viewSlotLevelAttrParent;
        [SerializeField] private Transform viewSlotLevelAttrIconParent;

        [Space] [Space]
        [SerializeField] private ViewSlotElementalInfo[] viewSlotElementalInfos;

        [Space] [Space]
        [SerializeField] private ActiveButton levelUpButton;
        [SerializeField] private Button enhanceButton;
        [SerializeField] private ViewGood enhanceViewGood;
        [SerializeField] private TMP_Text enhanceProbabilityTMP;

        [Space] [Space] 
        [SerializeField] private GameObject bottomView;
        [SerializeField] private GameObject notAcquiredView;
        
        public ViewCanvasElementalInfo SetTag(string text)
        {
            tagTMP.text = text;
            return this;
        }
        public ViewCanvasElementalInfo SetEnhanceProbabilityText(string text)
        {
            enhanceProbabilityTMP.text = text;
            return this;
        }

        public ViewCanvasElementalInfo SetAcquired(bool flag)
        {
            bottomView.SetActive(flag);
            notAcquiredView.SetActive(!flag);
            return this;
        }

        public ViewCanvasElementalInfo SetActiveWrap(bool isAbilityWrapOn)
        {
            abilityGo.SetActive(isAbilityWrapOn);
            infoGo.SetActive(!isAbilityWrapOn);
            return this;
        }
    }
}