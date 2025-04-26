using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasRanking : ViewCanvas
    {
        public Transform SlotParent => slotParent;
        public ViewSlotRanking MyViewSlotRanking => myViewSlotRanking;
        public SlideButton SlideButton => slideButton;
        
        [SerializeField] private Transform slotParent;
        [SerializeField] private ViewSlotRanking myViewSlotRanking;
        [SerializeField] private SlideButton slideButton;
    }
}