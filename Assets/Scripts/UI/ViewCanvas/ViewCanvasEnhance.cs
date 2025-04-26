using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Gradient = UnityEngine.Gradient;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasEnhance : ViewCanvas
    {
        public ViewSlotUI ViewSlotUI => viewSlotUI;
        public ViewGood ViewGood => viewGood;
        public ActiveButton EnhanceButton => enhacneButton;

        [SerializeField] private TMP_Text enhanceProbabilityTMP;
        [SerializeField] private ViewSlotUI viewSlotUI;
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private ActiveButton enhacneButton;

        [Space] [Space] [Header("Color")]
        [SerializeField] private Color successColor;
        [SerializeField] private Color failColor;

        public ViewCanvasEnhance SetEnhanceProbabilityText(string text)
        {
            enhanceProbabilityTMP.text = text;
            return this;
        }

        public Color GetEffectColor(bool isSuccess)
        {
            return isSuccess ? successColor : failColor;
        }
    }
}