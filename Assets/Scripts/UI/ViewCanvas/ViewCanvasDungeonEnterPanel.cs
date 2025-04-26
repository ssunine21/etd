using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasDungeon
    {
        public ViewCanvasPopup EnterPanel => enterPanel;
        public ViewGood[] RewardViewGoods => rewardViewGoods;
        public Button CloseButton => closeButton;
        public Button EnterButton => enterButton;
        public Button SweepButton => sweepButton;

        public GameObject SweepPanel => sweepPanel;
        public Image SweepBackground => sweepBackground;

        public RectTransform SweepRectTransform
        {
            get
            {
                _sweepRectTransform ??= sweepWrap.GetComponent<RectTransform>();
                return _sweepRectTransform;
            }
        }

        public CanvasGroup SweepCanvasGroup
        {
            get
            {
                _sweepCanvasGroup ??= sweepWrap.GetComponent<CanvasGroup>();
                return _sweepCanvasGroup;
            }
        }

        public Button SweepConfirmButton => sweepConfirmButton;
        public Button[] SweepCloseButtons => sweepCloseButtons;
        public CountButton CountButton => countButton;
        public ViewGood[] SweepRewardViewGoods => sweepRewardViewGoods;
        public ViewGood SweepNeededRewardGood => sweepNeedViewGood;
        public CheckBox NextChallengeCheckBox => nextChallengeCheckBox;
        
        [Space] [Space] [Header("EnterPanel")]       
        [SerializeField] private ViewCanvasPopup enterPanel;
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text titleShadowTMP;
        [SerializeField] private TMP_Text descTMP;
        [SerializeField] private Image blurryBannerImage;
        [SerializeField] private TMP_Text maxGoalTMP;
        [SerializeField] private ViewGood[] rewardViewGoods;
        [SerializeField] private GameObject nextChallengeGo;
        [SerializeField] private CheckBox nextChallengeCheckBox;

        [Space]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button enterButton;
        [SerializeField] private Button sweepButton;

        [SerializeField] private ViewGood[] needViewGoods;

        [Space] [Space] [Header("SweepPanel")]
        [SerializeField] private GameObject sweepPanel;
        [SerializeField] private Image sweepBackground;
        [SerializeField] private GameObject sweepWrap;
        [SerializeField] private ViewGood[] sweepRewardViewGoods;
        [SerializeField] private CountButton countButton;
        [SerializeField] private Button sweepConfirmButton;
        [SerializeField] private Button[] sweepCloseButtons;
        [SerializeField] private ViewGood sweepNeedViewGood;

        private RectTransform _sweepRectTransform;
        private CanvasGroup _sweepCanvasGroup;

        public ViewCanvasDungeon SetTitle(string text)
        {
            titleTMP.text = text;
            titleShadowTMP.text = text;
            return this;
        }
        
        public ViewCanvasDungeon SetDesc(string text)
        {
            descTMP.text = text;
            return this;
        }
        
        public ViewCanvasDungeon SetActiveNextChallenge(bool flag)
        {
            nextChallengeGo.SetActive(flag);
            return this;
        }
        
        public ViewCanvasDungeon SetBannerSprite(Sprite sprite)
        {
            blurryBannerImage.sprite = sprite;
            return this;
        }
        public ViewCanvasDungeon SetMaxGoal(string text)
        {
            maxGoalTMP.text = text;
            return this;
        }

        public ViewCanvasDungeon SetNeedGood(GoodType goodType)
        {
            foreach (var needViewGood in needViewGoods)
            {
                needViewGood.SetInit(goodType);
                needViewGood.SetValue(1);
            }
            return this;
        }
    }
}