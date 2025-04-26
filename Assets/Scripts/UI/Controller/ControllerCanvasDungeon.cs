using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasDungeon : ControllerCanvas
    {
        public int MenuIndex => 3;
        private ViewCanvasDungeon View => ViewCanvas as ViewCanvasDungeon;

        public ControllerCanvasDungeon(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasDungeon>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            
            foreach (var viewSlotDungeon in View.ViewSlotDungeons)
            {
                viewSlotDungeon.EnterButton.onClick.AddListener(() =>
                {
                    View.EnterPanel.Open();
                    UpdateEnterView(viewSlotDungeon.stageType);
                });
            }
            
            InitEnterPanel();
            UpdateDungeonSlot();
            
            DataController.Instance.good.OnBindChangeGood += UpdateDungeonSlotGoodValue;
            StageManager.Instance.onBindChangeStageType += UpdateChangeStage;
            StageManager.Instance.onBindStageEnd += (_, _) => UpdateDungeonGoal();
            
        }
        
        public RectTransform GetGuideArrowParent(QuestType questType)
        {
            var dungeonType = questType switch
            {
                QuestType.ClearGoldDungeon => StageType.GoldDungeon,
                QuestType.ClearDiaDungeon => StageType.DiaDungeon,
                QuestType.ClearEnhanceDungeon => StageType.EnhanceDungeon,
                _ => StageType.GoldDungeon
            };

            foreach (var viewSlotDungeon in View.ViewSlotDungeons)
            {
                if (viewSlotDungeon.stageType == dungeonType)
                {
                    return viewSlotDungeon.GuideArrowRectTransform;
                }
            }
            return View.ViewSlotDungeons[0].GuideArrowRectTransform;
        }

        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            if (View.EnterPanel.isActiveAndEnabled)
            {
                return tutorialType switch
                {
                    TutorialType.GoldDungeon => View.EnterButton.gameObject,
                    TutorialType.DiaDungeon => View.EnterButton.gameObject,
                    TutorialType.EnhanceDungeon => View.EnterButton.gameObject,
                    _ => null
                };
            }

            return tutorialType switch
            {
                TutorialType.GoldDungeon => View.ViewSlotDungeons[0].gameObject,
                TutorialType.DiaDungeon => View.ViewSlotDungeons[1].gameObject,
                TutorialType.EnhanceDungeon => View.ViewSlotDungeons[2].gameObject,
                _ => null
            };
        }

        private void UpdateDungeonSlot()
        {
            UpdateDungeonGoal();
            foreach (var slotDungeon in View.ViewSlotDungeons)
            {
                UpdateDungeonSlotGoodValue(DataController.Instance.dungeon.GetTicketGoodType(slotDungeon.stageType));
            }
        }

        private void UpdateDungeonGoal()
        {
            foreach (var slotDungeon in View.ViewSlotDungeons)
            {
                var dungeonType = slotDungeon.stageType;
                slotDungeon
                    .SetGoalText(LocalizeManager.GetDungeonGoalTextWithCount(dungeonType, DataController.Instance.dungeon.GetMaxGoalCount(dungeonType)));
            }
        }

        private void UpdateDungeonSlotGoodValue(GoodType goodType)
        {
            foreach (var slotDungeon in View.ViewSlotDungeons)
            {
                var dungeonType = slotDungeon.stageType;
                if(DataController.Instance.dungeon.GetTicketGoodType(dungeonType) == goodType)
                {
                    var goodValue = DataController.Instance.good.GetValue(goodType);
                    slotDungeon.TicketViewGood
                        .SetInit(goodType)
                        .SetValue(goodValue)
                        .SetValue($"{goodValue} / {2}");
                }
            }
        }
        
        private void UpdateChangeStage(StageType stageType, int param0)
        {
            if (StageManager.Instance.PrevPlayingStageType is StageType.GoldDungeon or StageType.DiaDungeon or StageType.EnhanceDungeon or StageType.Normal)
            {
                View.SetActive(stageType == StageType.Normal);
                View.EnterPanel.SetActive(stageType == StageType.Normal);
            }
        }
    }
}