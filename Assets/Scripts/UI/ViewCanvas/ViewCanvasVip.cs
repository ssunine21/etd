using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasVip : ViewCanvas
    {
        public Button GetRewardButton => getRewardButton;
        public Button GetAdsRewardButton => getAdsRewardButton;
        public Image BoxCap => boxCapImage;
        public Image Box => boxImage;
        public ViewSlotVIP ViewSlotVip => viewSlotVip;
        public Button CheckNextVipButton => checkNextVip;
        public ReddotView Reddot => reddot;
        
        [SerializeField] private ViewSlotVIP viewSlotVip;
        [SerializeField] private Button getRewardButton;
        [SerializeField] private Button getAdsRewardButton;
        [SerializeField] private Button checkNextVip;
        [SerializeField] private Image boxImage;
        [SerializeField] private Image boxCapImage;
        [SerializeField] private Sprite[] boxSprites;
        [SerializeField] private TMP_Text timeTMP;
        [SerializeField] private TMP_Text benefitTMP;
        [SerializeField] private TMP_Text rewardTMP;
        [SerializeField] private TMP_Text checkNextVipTMP;
        [SerializeField] private GameObject emptyText;
        [SerializeField] private ReddotView reddot;

        public ViewCanvasVip SetBenefitTitleText(string text)
        {
            benefitTMP.text = text;
            return this;
        }

        public ViewCanvasVip SetRewardTitleText(string text)
        {
            rewardTMP.text = text;
            return this;
        }
        
        public ViewCanvasVip SetCheckNextVipText(string text)
        {
            checkNextVipTMP.text = text;
            return this;
        }
        
        public void SetBoxSprite(Sprite sprite)
        {
            boxImage.sprite = sprite;
        }

        public Sprite GetBoxSprite(bool isOpen)
        {
            return boxSprites[isOpen ? 0 : 1];
        }

        public void SetTimeText(string text)
        {
            timeTMP.text = text;
        }

        public void SetActiveEmptyText(bool flag)
        {
            emptyText.SetActive(flag);
        }
    }
}