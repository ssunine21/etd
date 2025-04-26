using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMainMenu
    {
        public RectTransform QuestPanel => questPanel;
        public Button QuestArrowButton => questArrowButton;
        public ViewGood QuestViewGood => questViewGood;
        public Image EffectImage => effectImage;
        public Button QuestClearButton => questClearButton;
        public RectTransform QuestGuideArrowRectTransform => questGuideArrowRectTransform;

        [Space] [Space]
        [Header("Quest")]
        [SerializeField] private RectTransform questPanel;
        [SerializeField] private Image effectImage;
        [SerializeField] private Image countFillAmount;
        [SerializeField] private Button questClearButton;
        [SerializeField] private Button questArrowButton;
        [SerializeField] private ViewGood questViewGood;
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text levelTMP;
        [SerializeField] private RectTransform questGuideArrowRectTransform;

        public ViewCanvasMainMenu SetQuestLevel(string text)
        {
            levelTMP.text = text;
            return this;
        }
        
        public ViewCanvasMainMenu SetQuestTitle(string text)
        {
            titleTMP.text = text;
            return this;
        }

        public ViewCanvasMainMenu SetCountFillAmount(int curr, int max)
        {
            countFillAmount.fillAmount = curr == 0 || max == 0 ? 0 : (float)curr / max;
            return this;
        }
    }
}