using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasDungeonStage : ViewCanvas
    {
        public Button BackButton => backButton;
        public ViewGood EarnedRewardViewGoodPrefab => earnedRewardViewGoodPrefab;
        public Transform EarnedRewardViewParent => earnedRewardViewParent;
        
        [Header("Top")]
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private Image badgeImage;
        [SerializeField] private TMP_Text levelTMP;
        [SerializeField] private Image gageFillAmount;
        [SerializeField] private RawImage gageImage;
        [SerializeField] private Image timerFillAmount;
        [SerializeField] private TMP_Text timerTMP;
        [SerializeField] private ViewGood earnedRewardViewGoodPrefab;
        [SerializeField] private Transform earnedRewardViewParent;
        
        [Space] [Space]
        [Header("Mid")]
        [SerializeField] private TMP_Text countDescTMP;
        [SerializeField] private TMP_Text countTMP;

        [Space] [Space] 
        [Header("Bottom")] 
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text descriptionTMP;
        
        [Space] [Space]
        [Header("Top Resources")]
        [SerializeField] private Sprite[] badgeSprites;
        [SerializeField] private Texture[] gageTextures;
        
        public ViewCanvasDungeonStage SetTitle(string text)
        {
            titleTMP.text = text;
            return this;
        }
        
        public ViewCanvasDungeonStage SetLevelText(string text)
        {
            levelTMP.text = text;
            return this;
        }

        public ViewCanvasDungeonStage SetGageTexture(StageType stageType, Color? color = null)
        {
            var sprite = gageTextures[(int)stageType];
            return SetGageTexture(sprite, color);
        }

        private ViewCanvasDungeonStage SetGageTexture(Texture texture, Color? color = null)
        {
            gageImage.texture = texture;
            gageImage.color = color.GetValueOrDefault(Color.white);
            return this;
        }

        public ViewCanvasDungeonStage SetGageFillAmount(float amount)
        {
            gageFillAmount.fillAmount = amount;
            return this;
        }

        public ViewCanvasDungeonStage SetTimerFillAmount(float amount)
        {
            timerFillAmount.fillAmount = amount;
            return this;
        }

        public ViewCanvasDungeonStage SetTimerText(string text, bool isTimer = true)
        {
            timerTMP.text = text;
            timerTMP.gameObject.SetActive(isTimer);
            return this;
        }

        public ViewCanvasDungeonStage SetBadgeSprite(StageType stageType)
        {
            var sprite = badgeSprites[(int)stageType];
            return SetBadgeSprite(sprite);
        }

        private ViewCanvasDungeonStage SetBadgeSprite(Sprite sprite)
        {
            badgeImage.sprite = sprite;
            return this;
        }

        public ViewCanvasDungeonStage SetTotalCountDescText(string text)
        {
            countDescTMP.text = text;
            return this;
        }
        
        public ViewCanvasDungeonStage SetTotalCountText(string text)
        {
            countTMP.text = text;
            return this;
        }

        public ViewCanvasDungeonStage SetDescription(string text)
        {
            descriptionTMP.text = text;
            return this;
        }
    }
}