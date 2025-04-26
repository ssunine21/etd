using System;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotProduct : MonoBehaviour
    {
        public ProductType ProductType => type;
        public ActiveButton Button => button;
        public ViewGood ViewGood => viewGood;
        public ScrollRect RewardScrollRect => rewardScrollRect;

        public ViewGood ViewGoodTooltipPrefab
        {
            get
            {
                viewGoodTooltipPrefab.SetActive(false);
                return viewGoodTooltipPrefab;
            }
        }
        
        public bool IsActiveTimeLockPanel => timeLockPanel.activeSelf;
        public Image Icon => icon;
        public ReddotView Reddot => reddot;
        
        [Header("Commom")]
        [SerializeField] private ProductType type;
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private ActiveButton button;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject bookmark;
        [SerializeField] private TMP_Text bookmarkTextTMP;

        [Space] [Space] [Header("Package")] 
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private GameObject goPackageValue;
        [SerializeField] private TMP_Text packageValueTMP;
        [SerializeField] private ViewGood viewGoodTooltipPrefab;
        [SerializeField] private ScrollRect rewardScrollRect;
        [SerializeField] private TMP_Text priceTMP;
        [SerializeField] private TMP_Text multiplePriceTMP;
        
        [Space][Space] [Header("TimeAttack")]
        [SerializeField] private GameObject timeLockPanel;
        [SerializeField] private TMP_Text timeTMP;
        [SerializeField] private TMP_Text purchaseCountTMP;
        [SerializeField] private TMP_Text rewardCountTMP;

        [Space] [Space] 
        [SerializeField] private ReddotView reddot;
        
        
        public ViewSlotProduct SetTitle(string text)
        {
            if (titleTMP)
                titleTMP.text = text;
            return this;
        }

        public ViewSlotProduct SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
            return this;
        }

        public ViewSlotProduct SetIcon(GoodType goodType)
        {
            var sprite = DataController.Instance.good.GetImage(goodType);
            return SetIcon(sprite);
        }

        public ViewSlotProduct SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotProduct SetPurchaseCountText(string text)
        {
            if (purchaseCountTMP)
                purchaseCountTMP.text = text;
            return this;
        }

        public ViewSlotProduct SetRewardText(string text)
        {
            if (rewardCountTMP)
                rewardCountTMP.text = text;
            return this;
        }

        public ViewSlotProduct SetTimeLockPanel(bool flag)
        {
            if (timeLockPanel)
                timeLockPanel.SetActive(flag);
            return this;
        }

        public ViewSlotProduct SetRemainingTimeText(string text)
        {
            if (timeTMP)
                timeTMP.text = text;
            return this;
        }

        public ViewSlotProduct SetPackageValue(float value)
        {
            if (!goPackageValue) return this;
            if (!packageValueTMP) return this;
            
            goPackageValue.SetActive(value >= 1);

            var text = LocalizeManager.GetText(LocalizedTextType.Shop_PackageValue, value);
            var textSplit = text.Replace(",", "").Split(" ");
            try
            {
                packageValueTMP.text = $"<size=130%>{textSplit[0]}</size>{textSplit[1]}";
            }
            catch (Exception e)
            {
                goPackageValue.SetActive(false);
            }
            
            return this;
        }

        public ViewSlotProduct SetInAppPriceText(string text)
        {
            if (priceTMP)
                priceTMP.text = text;
            return this;
        }

        public ViewSlotProduct SetMultiplePriceText(string text)
        {
            if (multiplePriceTMP)
                multiplePriceTMP.text = text;
            return this;
        }

        public ViewSlotProduct SetActiveViewGood(bool flag)
        {
            if (viewGood != null)
                viewGood.gameObject.SetActive(flag);
            if (priceTMP != null)
                priceTMP.gameObject.SetActive(!flag);
            return this;
        }
        

        public ViewSlotProduct SetActiveBookmark(bool flag)
        {
            if(bookmark) bookmark.SetActive(flag);
            return this;
        }

        public ViewSlotProduct SetBookmarkText(string text)
        {
            if (bookmarkTextTMP) bookmarkTextTMP.text = text;
            return this;
        }
    }
}