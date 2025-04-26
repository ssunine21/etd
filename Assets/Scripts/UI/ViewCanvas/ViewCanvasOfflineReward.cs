using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasOfflineReward : ViewCanvas
    {
        public Button GetRewardButton => getRewardButton;
        public Button Get2XRewardButton => get2XRewardButton;
        public Transform SlotParent => slotParent;
        public RectTransform DoubleTMP => doubleTMP;
        public ViewSlotTime ViewSlotTime => viewSlotTime;
        
        [SerializeField] private TMP_Text descTMP;
        [SerializeField] private Button getRewardButton;
        [SerializeField] private Button get2XRewardButton;
        [SerializeField] private Transform slotParent;
        [SerializeField] private RectTransform doubleTMP;
        [SerializeField] private ViewSlotTime viewSlotTime;

        public ViewCanvasOfflineReward SetDescriptionText(string text)
        {
            descTMP.text = text;
            return this;
        }
    }
}