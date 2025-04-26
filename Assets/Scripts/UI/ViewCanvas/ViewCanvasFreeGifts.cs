using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasFreeGifts : ViewCanvas
    {
        public ViewSlotProduct ViewSlotProduct => viewSlotProduct;
        public HorizontalLayoutGroup[] Slots => slots;
        public ViewSlotTime ViewSlotTime => viewSlotTime;
        
        [SerializeField] private ViewSlotTime viewSlotTime;
        [SerializeField] private ViewSlotProduct viewSlotProduct;
        [SerializeField] private HorizontalLayoutGroup[] slots;
        [SerializeField] private TMP_Text timeTMP;
    }
}