using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasPass : ViewCanvas
    {
        public ActiveButton PremiumPurchaseActiveButton => premiumPurchaseActiveButton;
        public ActiveButton MasterurPchaseActiveButton => masterurPchaseActiveButton;
        public Transform SlotParent => slotParent;
        public Button OpenGuideButton => guideButton;
        public ViewCanvasPopup GuideViewCanvasPopup => guideViewCanvasPopup;
        public ViewCanvasPopup ElementalSelectedViewCanvasPopup => elementalSelectedViewCanvasPopup;
        public ViewSlotUI[] ViewSlotUis => viewSlotUIs;
        public ActiveButton GetRewardSelectedElementalButton => getRewardSelectedElementalButton;
        public ViewSlotTime ViewSlotTime => viewSlotTime;
        public ScrollRect ScrollRect => scrollRect;
        
        [SerializeField] private ViewSlotTime viewSlotTime;
        [SerializeField] private TMP_Text levelTMP;
        [SerializeField] private TMP_Text expTMP;
        [SerializeField] private Image expFillAmount;
        [SerializeField] private Transform slotParent;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private ActiveButton premiumPurchaseActiveButton;
        [SerializeField] private ActiveButton masterurPchaseActiveButton;
        
        [Space][Space][Header("Guide")]
        [SerializeField] private Button guideButton;
        [SerializeField] private ViewCanvasPopup guideViewCanvasPopup;
        [SerializeField] private TMP_Text guideDescriptionTMP;
            
        [FormerlySerializedAs("elementalSelectedViewPopup")]
        [Space][Space][Header("Reward Elemental")]
        [SerializeField] private ViewCanvasPopup elementalSelectedViewCanvasPopup;
        [SerializeField] private ViewSlotUI[] viewSlotUIs;
        [SerializeField] private ActiveButton getRewardSelectedElementalButton;
        
        public ViewCanvasPass SetGuideDescription(string text)
        {
            guideDescriptionTMP.text = text;
            return this;
        }

        public ViewCanvasPass SetLevel(int level)
        {
            return SetLevel(level.ToString());
        }

        public ViewCanvasPass SetExpFillAmount(float amount)
        {
            expFillAmount.fillAmount = amount;
            return this;
        }

        public ViewCanvasPass SetExp(string text)
        {
            expTMP.text = text;
            return this;
        }

        private ViewCanvasPass SetLevel(string text)
        {
            levelTMP.text = text;
            return this;
        }
    }
}