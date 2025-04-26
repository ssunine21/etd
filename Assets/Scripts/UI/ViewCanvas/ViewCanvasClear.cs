using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasClear : ViewCanvas
    {
        public ActiveButton Button0 => activeButton0;
        public ActiveButton Button1 => activeButton1;
        public ViewGood MyViewGood => myViewGood;
        public ViewGood[] NeededViewGoods => neededViewGoods;
        public Transform RewardViewGoodParent => rewardViewGoodParent;
        public Transform ViewSlotAddValueParent => viewSlotAddValueParent;
        public ViewSlotTitle ViewSlotTitle => viewSlotTitle;

        [SerializeField] private ViewSlotTitle viewSlotTitle;
        [SerializeField] private ActiveButton activeButton0;
        [SerializeField] private ActiveButton activeButton1;
        [SerializeField] private ViewGood myViewGood;
        [SerializeField] private ViewGood[] neededViewGoods;
        [SerializeField] private Transform rewardViewGoodParent;
        [SerializeField] private Transform viewSlotAddValueParent;
        [SerializeField] private TMP_Text closeSecondsTMP;

        public ViewCanvasClear SetCloseSecondsText(string text)
        {
            closeSecondsTMP.text = text;
            return this;
        }
    }
}