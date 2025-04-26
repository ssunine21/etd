using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasStage : ViewCanvas
    {
        public Image ProgressFillAmount => progressFillAmount;
        public BackgroundRolling BackgroundRolling => backgroundRolling;
        public ViewCanvasPopup StageListViewCanvasPopup => stageListViewCanvasPopup;
        public Button StageListButton => stageListButton;
        public Button GoToMaxStage => goToMaxStageButton;

        public ScrollRect StageScrollRect => stageScrollRect;
        public Button GoCurrStageButton => goCurrStageButton;
        public Button NextStageButton => nextStageButton;
        public Button PrevStageButton => prevStageButton;

        [SerializeField] private ViewSlotTitle viewSlotStageListTitle;
        [SerializeField] private TMP_Text currStageText;
        [SerializeField] private TMP_Text stageListLevelTMP;
        [SerializeField] private TMP_Text stageListLevelExpressTMP;
        [SerializeField] private Transform repeatArrowTr;
        [SerializeField] private Image progressFillAmount;
        [SerializeField] private BackgroundRolling backgroundRolling;

        [SerializeField] private Button goToMaxStageButton;
        [SerializeField] private GameObject viewGoToBoss;
        [SerializeField] private GameObject viewGoToMaxStage;
        [SerializeField] private GameObject viewBossStage;
        [SerializeField] private Image imageDisabledView;

        [FormerlySerializedAs("stageListViewPopup")] [SerializeField] private ViewCanvasPopup stageListViewCanvasPopup;
        [SerializeField] private Button stageListButton;

        [SerializeField] private ScrollRect stageScrollRect;
        
        [SerializeField] private Button goCurrStageButton;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Button prevStageButton;

        private Tween _tween;

        public ViewCanvasStage SetStageListTitle(string title)
        {
            viewSlotStageListTitle.SetText(title);
            return this;
        }
        
        public ViewCanvasStage SetStageListExpression(string title)
        {
            stageListLevelExpressTMP.SetText(title);
            return this;
        }

        public ViewCanvasStage SetStageListLevelTitle(string title)
        {
            stageListLevelTMP.text = title;
            return this;
        }
        
        public ViewCanvasStage SetStageText(string text)
        {
            currStageText.text = text;
            return this;
        }

        public ViewCanvasStage SetActiveRepeatArrow(bool flag)
        {
            repeatArrowTr.gameObject.SetActive(flag);
            return this;
        }

        public ViewCanvasStage SetStageView(bool isMaxStage, bool isMaxStageTheBossStage)
        {
            viewGoToBoss.SetActive(!isMaxStage && isMaxStageTheBossStage);
            viewGoToMaxStage.SetActive(!isMaxStageTheBossStage);
            viewBossStage.SetActive(isMaxStage && isMaxStageTheBossStage);
            imageDisabledView.enabled = isMaxStage && !isMaxStageTheBossStage;

            _tween ??= goToMaxStageButton.transform
                .DOPunchScale(Vector2.one * 0.07f, 1f, 1, 0)
                .SetLoops(int.MaxValue)
                .SetUpdate(true);

            if (isMaxStage)
            {
                _tween.Pause();
                goToMaxStageButton.transform.localScale = Vector3.one;
            }
            else
            {
                if (!_tween.IsPlaying())
                    _tween.Play();
            }

            return this;
        }
    }
}