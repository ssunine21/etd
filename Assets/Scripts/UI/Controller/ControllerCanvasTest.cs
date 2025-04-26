using System;
using System.Collections.Generic;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasTest : ControllerCanvas
    {
        enum TestMoveType
        {
            Stage,
            Quest
        }
        
        public static bool IsEnemyInvincibility { get; set; }
        public static bool IsUnitInvincibility { get; set; }

        private ViewCanvasTest View => ViewCanvas as ViewCanvasTest;
        private TestMoveType _moveType;
    
        public ControllerCanvasTest(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasTest>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.InitPanel();
        
            View.GoodViewCloseButton.onClick.AddListener(() => View.GoodView.SetActive(false));
            View.StageClearButton.onClick.AddListener(() =>
            {
                _moveType = TestMoveType.Stage;
                View.QuestPanel.SetActive(true);
            });
            View.StageInitButton.onClick.AddListener(() =>
            {
                DataController.Instance.maxTotalLevel = 0;
                StageManager.Instance.MoveToNormalStageLevel(0);
            });
            
            View.MoveQuestPanel.onClick.AddListener(() =>
            {
                _moveType = TestMoveType.Quest;
                View.QuestPanel.SetActive(true);
            });
            
            View.MoveQuest.onClick.AddListener(() =>
            {
                if(_moveType == TestMoveType.Quest)
                {
                    DataController.Instance.quest.currQuestLevel = int.Parse(View.QuestInputField.text) - 1;
                    DataController.Instance.quest.TryClear();
                }
                else if (_moveType == TestMoveType.Stage)
                {
                    DataController.Instance.stage.SaveStageLevelData(int.Parse(View.QuestInputField.text));
                    StageManager.Instance.StageClear();
                }
                Close();
            });
        
            View.GoodButton.onClick.AddListener(() => View.GoodView.SetActive(true));
            View.ShowTransientMessage.onClick.AddListener(() => Get<ControllerCanvasToastMessage>().ShowTransientToastMessage("테스트 메시지 입니다."));
            View.ShowInitPanelButton.onClick.AddListener(View.ShowInitPanel);
        
            View.UpgradeInitButton.onClick.AddListener(() => DataController.Instance.upgrade.InitUpgradeLevel());
            View.ResearchInitButton.onClick.AddListener(() => DataController.Instance.research.InitResearchLevel());
            View.NextTimeButton.onClick.AddListener(() => ServerTime.onBindNextDay?.Invoke());
            View.AllInitButton.onClick.AddListener(() => {DataController.Instance.ForceInit();});
            View.BackButton.onClick.AddListener(View.InitPanel);
            View.ReleaseGrowPass.onClick.AddListener(() =>
            {
                DataController.Instance.elemental.summonCount += 10000;
                DataController.Instance.rune.summonCount += 10000;
            });
        
            View.ElementalInitButton.onClick.AddListener(InitElementals);
            View.RuneInitButton.onClick.AddListener(InitRunes);
            View.EarnAllElemental.onClick.AddListener(() =>
            {
                foreach (ElementalType elementalType in Enum.GetValues(typeof(ElementalType)))
                {
                    foreach (GradeType gradeType in Enum.GetValues(typeof(GradeType)))
                    {
                        if(gradeType == GradeType.SSS) continue;
                        DataController.Instance.elemental.Earn(new KeyValuePair<ElementalType, GradeType>(elementalType, gradeType));
                    }
                }
            });
            
            foreach (GoodType goodType in Enum.GetValues(typeof(GoodType)))
            {
                if(goodType == GoodType.None) continue;
            
                var good = new GameObject(goodType.ToString())
                {
                    transform =
                    {
                        parent = View.GoodScrollRect.content,
                        localScale = Vector3.one
                    }
                };

                var goodSprite = good.AddComponent<Image>();
                goodSprite.sprite = DataController.Instance.good.GetImage(goodType);

                var earnButton = good.AddComponent<Button>();
                earnButton.onClick.AddListener(() =>
                {
                    DataController.Instance.good.Earn(goodType, double.Parse(View.GoodInputField.text));
                });
            }

            IsEnemyInvincibility = View.EnemyInvincibilityToggle.isOn;
            var toEnemyEvent = View.EnemyInvincibilityToggle.onValueChanged;
            toEnemyEvent.AddListener((isOn) =>
            {
                IsEnemyInvincibility = isOn;
            });
            IsUnitInvincibility = View.UnitInvincibilityToggle.isOn;
            var toUnitEvent = View.UnitInvincibilityToggle.onValueChanged;
            toUnitEvent.AddListener((isOn) =>
            {
                IsUnitInvincibility = isOn;
            });
        }

        private void InitElementals()
        {
            foreach (var elemental in DataController.Instance.elemental.Gets())
            {
                elemental.equippedIndex = -1;
            }
            DataController.Instance.elemental.elementals?.Clear();
            DataController.Instance.player.OnBindChangedElemental?.Invoke(0);
            DataController.Instance.player.OnBindChangedElemental?.Invoke(1);
            DataController.Instance.player.OnBindChangedElemental?.Invoke(2);
        }

        private void InitRunes()
        {
            foreach (var rune in DataController.Instance.rune.runes)
            {
                rune.equippedIndex = -1;
            }
            DataController.Instance.rune.runes?.Clear();
            DataController.Instance.player.OnBindChangedRune?.Invoke(0);
            DataController.Instance.player.OnBindChangedRune?.Invoke(1);
            DataController.Instance.player.OnBindChangedRune?.Invoke(2);
        }
    }
}