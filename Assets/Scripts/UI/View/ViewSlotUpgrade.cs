using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotUpgrade : MonoBehaviour
    {
        public UpgradeType UpgradeType { get; set; }
        public ActiveButton UpgradeButton => _upgradeActiveButton;
        public RectTransform LightSmall => lightSmall;
        public RectTransform LightBig => lightBig;
        public RectTransform GuideArrowRectTransform => guideArrowRectTransform;

        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _contentsText;
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private TMP_Text value;
        [SerializeField] private TMP_Text maxTMP;
        [SerializeField] private GameObject upgradeButtonContainer;
        [SerializeField] private ActiveButton _upgradeActiveButton;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image backgroundStrokeImage;

        [SerializeField] private RectTransform lightSmall;
        [SerializeField] private RectTransform lightBig;

        [SerializeField] private RectTransform guideArrowRectTransform;
        [SerializeField] private GameObject lockPanel;
        [SerializeField] private TMP_Text lockPanelTMP;
        
        public ViewSlotUpgrade SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
            return this;
        }
    
        public ViewSlotUpgrade SetContentsText(string text)
        {
            _contentsText.text = text;
            return this;
        }

        public ViewSlotUpgrade SetLevel(string level)
        {
            _levelText.text = level;
            return this;
        }

        public ViewSlotUpgrade SetValue(string valueString)
        {
            value.text = valueString;
            return this;
        }

        public ViewSlotUpgrade SetLockPanel(bool flag, string text = "")
        {
            lockPanel.SetActive(flag);
            if (!string.IsNullOrEmpty(text))
                lockPanelTMP.text = text;

            return this;
        }

        public ViewSlotUpgrade SetGoodType(GoodType goodType)
        {
            viewGood.SetInit(goodType);
            return this;
        }

        public ViewSlotUpgrade SetGoodValue(double goodValue)
        {
            viewGood.SetValue(goodValue);
            return this;
        }

        public ViewSlotUpgrade SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotUpgrade SetMaxLevel(bool flag)
        {
            var backgroundColor = flag ? new Color(6 / 255f, 26 / 255f, 35 / 255f) : new Color(17 / 255f, 55 / 255f, 74 / 255f);
            var strokeColor = flag ? new Color(17 / 255f, 55 / 255f, 74 / 255f) : new Color(87 / 255f, 111 / 255f, 114 / 255f);
            maxTMP.gameObject.SetActive(flag);
            upgradeButtonContainer.SetActive(!flag);

            backgroundImage.color = backgroundColor;
            backgroundStrokeImage.color = strokeColor;
            return this;
        }
    }
}