using ETD.Scripts.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasMail : ViewCanvas
    {
        public Transform SlotParent => slotParent;
        public ActiveButton GetAllRewardButton => getAllRewardButton;

        [SerializeField] private Transform slotParent;
        [SerializeField] private ActiveButton getAllRewardButton;
        [SerializeField] private GameObject emptyTextPanel;

        public ViewCanvasMail SetActiveEmptyTextPanel(bool flag)
        {
            emptyTextPanel.SetActive(flag);
            return this;
        }
    }
}