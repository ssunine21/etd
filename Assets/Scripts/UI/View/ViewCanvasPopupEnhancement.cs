using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.ViewCanvas;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewCanvasPopupEnhancement : ViewCanvasPopup
    {
        public TMP_Text EnhanceProbabilityText => enhanceProbabilityText;
        public ViewSlotUI ViewSlotUI => viewSlotUI;
        public ViewGood ViewGood => viewGood;
        public ActiveButton EnhancementButton => enhancementButton;

        [SerializeField] private TMP_Text enhanceProbabilityText;
        [SerializeField] private ViewSlotUI viewSlotUI;
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private ActiveButton enhancementButton;
    }
}