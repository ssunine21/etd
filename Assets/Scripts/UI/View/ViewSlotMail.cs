using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotMail : ViewSlot<ViewSlotMail>
    {
        public ViewGood ViewGood => viewGood;
        public Button GetRewardButton => rewardButton;
        
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text descTMP;
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private Button rewardButton;
        [SerializeField] private TMP_Text remainingTimeTMP;

        private string _inDate;
        
        public ViewSlotMail SetTitle(string text)
        {
            titleTMP.text = text;
            return this;
        }
        public ViewSlotMail SetDescription(string text)
        {
            descTMP.text = text;
            return this;
        }

        public ViewSlotMail SetRemainingTime(string text)
        {
            remainingTimeTMP.text = text;
            return this;
        }

        public string GetInDate()
        {
            return _inDate;
        }

        public ViewSlotMail SetInDate(string text)
        {
            _inDate = text;
            return this;
        }
        
        public void ReceivePostItem()
        {
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
            BackendManager.ReceivePostItem(GetInDate(), reward =>
            {
                var goodItem = new GoodItem(reward.Key, reward.Value);

                ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(goodItem, LocalizeManager.GetText(LocalizedTextType.Claimed));
                DataController.Instance.good.EarnReward(goodItem);
                
                SetActive(false);
                        
                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
                DataController.Instance.LocalSave();
            });
        }

    }
}
