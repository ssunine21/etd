using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasUpgrade : ViewCanvas
    {
        public ViewSlotUpgrade ViewSlotUpgradePrefab => _viewSlotUpgradePrefab;
        public ScrollRect SlotScrollRect => _slotScrollRect;
        public SlideButton SlideButton => slideButton;
        
        [SerializeField] private ViewSlotUpgrade _viewSlotUpgradePrefab;
        [SerializeField] private ScrollRect _slotScrollRect;

        [Space] [Space] [Header("Bottom")]
        [SerializeField] private SlideButton slideButton;
    }
}