using ETD.Scripts.UI.View;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasProbability : ViewCanvas
    {
        public Transform SlotParent => slotParent;
        
        [SerializeField] private Transform slotParent;
    }
}