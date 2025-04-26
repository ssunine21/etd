using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasMission : ViewCanvas
    {
        public float TotalGageWidth {
            get
            {
                if(!_totalGageRect)
                {
                    if (totalFillAmountGage.TryGetComponent(out _totalGageRect))
                        return _totalGageRect.rect.width;
                }

                return _totalGageRect.rect.width;
            }
        }
        public Transform TotalRewardSlotParent => totalRewardSlotParent;
        public Transform RewardSlotParent => rewardSlotParent;
        
        public Button AllEarnButton => allEarnButton;
        public SlideButton SlideButton => slideButton;
        public ViewSlotMissionTotalReward ViewSlotMissionTotalRewardPrefab => viewSlotMissionTotalRewardPrefab;
        public ViewSlotMissionReward ViewSlotMissionRewardPrefab => viewSlotMissionRewardPrefab;
        public ViewSlotTime ViewSlotTime => viewSlotTime;
        

        [Space] [Space] [Header("Total Rewards")] 
        [SerializeField] private GameObject goTotalReward;
        [SerializeField] private Transform totalRewardSlotParent;
        [SerializeField] private Image totalFillAmountGage;
        [SerializeField] private TMP_Text totalCountTMP;
        [SerializeField] private ViewSlotTime viewSlotTime;

        [Space] [Space] [Header("Rewards")]
        [SerializeField] private RectTransform rewardSlotScrollRect;
        [SerializeField] private Transform rewardSlotParent;
        
        [Space] [Space] [Header("Bottom")]
        [SerializeField] private Button allEarnButton;
        [SerializeField] private ReddotView allEarnButtonReddotView;
        [SerializeField] private SlideButton slideButton;

        [Space] [Space] [Header("Prefabs")]
        [SerializeField] private ViewSlotMissionTotalReward viewSlotMissionTotalRewardPrefab;
        [SerializeField] private ViewSlotMissionReward viewSlotMissionRewardPrefab;

        private RectTransform _totalGageRect;

        private void Start()
        {
            _totalGageRect = totalFillAmountGage.GetComponent<RectTransform>();
        }

        public ViewCanvasMission SetTotalAmount(float curr, float max)
        {
            SetTotalFillAmountGage(curr == 0 || max == 0 ? 0 : curr / max);
            totalCountTMP.text = $"{curr}/{max}";
            return this;
        }
        
        public ViewCanvasMission SetTotalFillAmountGage(float amount)
        {
            totalFillAmountGage.fillAmount = amount;
            return this;
        }

        public ViewCanvasMission ShowTotalRewardView(bool flag)
        {
            goTotalReward.SetActive(flag);
            rewardSlotScrollRect.offsetMax = new Vector2(0, flag ? -290 : 0);
            return this;
        }

        public ViewCanvasMission ShowAllEarnButtonReddot(bool flag)
        {
            allEarnButtonReddotView.ShowReddotIcon(flag);
            return this;
        }
    }
}