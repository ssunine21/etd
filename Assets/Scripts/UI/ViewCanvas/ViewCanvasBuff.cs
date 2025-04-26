using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasBuff : ViewCanvas
    {
        public ViewSlotBuff[] ViewSlotBuffs => viewSlotBuffs;
        
        [SerializeField] private ViewSlotBuff[] viewSlotBuffs;
        [SerializeField] private TMP_Text totalGameSpeedTMP;

        public void SetTotalGameSpeedText(string text)
        {
            totalGameSpeedTMP.text = text;
        }
    } 
}