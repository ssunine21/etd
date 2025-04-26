using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMainMenu
    {
        private bool _isOnQuestPanel;
        private Sequence _effectSequence;
        
        private void InitQuest()
        {
            View.QuestArrowButton.onClick.AddListener(() => OnOffQuestPanel(!_isOnQuestPanel));
            View.QuestClearButton.onClick.AddListener(() => DataController.Instance.quest.TryClear(isSuccess =>
            {
                if (!_isOnQuestPanel)
                {
                    OnOffQuestPanel(true);
                    return;
                }
                
                if(isSuccess)
                {
                    var reward = DataController.Instance.quest.GetReward();
                    var count = reward.Value < 30 ? (int)reward.Value : Random.Range(2, 6);
                    var viewGood = Get<ControllerCanvasMainMenu>().GetViewGood(reward.Key);
                    GoodsEffectManager.Instance.ShowEffect(reward.Key, View.QuestViewGood.ImageTr.position, viewGood, count);
                    DataController.Instance.good.Earn(reward.Key, reward.Value);
                    
                    DataController.Instance.LocalSave();
                }
                else
                {
                    Get<ControllerCanvasTutorial>().QuestGuide(DataController.Instance.quest.GetQuestType());
                }
            }));

            DataController.Instance.quest.OnBindCount += UpdateQuestInfomation;
            DataController.Instance.quest.OnBindClear += UpdateQuest;

            OnOffQuestPanel(true);
            UpdateQuest(DataController.Instance.quest.currQuestLevel);
        }

        private void OnOffQuestPanel(bool flag)
        {
            const float duration = 0.5f;
            var toX = flag ? -120f : 195f;
            var toZ = flag ? 0f : 180f;
            View.QuestViewGood.gameObject.SetActive(flag);
            View.QuestPanel.DOAnchorPosX(toX, duration).SetEase(Ease.OutBack).SetUpdate(true);
            View.QuestArrowButton.transform
                .DORotate(new Vector3(0, 0, toZ), duration * 1.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
            
            _isOnQuestPanel = flag;
        }

        private void UpdateQuest(int currQuestLevel)
        {
            if (DataController.Instance.quest.IsAllClear)
            {
                View.QuestPanel.gameObject.SetActive(false);
                return;
            }
            UpdateQuestReward();
            UpdateQuestInfomation();
        }

        private void UpdateQuestReward()
        {
            var reward = DataController.Instance.quest.GetReward();
            View.QuestViewGood
                .SetInit(reward.Key)
                .SetValue($"X {reward.Value.ToGoodString(reward.Key)}");
        }

        private void UpdateQuestInfomation()
        {
            var curr = DataController.Instance.quest.currCount;
            var goal = DataController.Instance.quest.GetGoalCount();
            var countText =
                DataController.Instance.quest.GetQuestType() == QuestType.ClearNormalStage
                    ? $"{DataController.Instance.stage.GetStageLevelExpression(goal)}"
                    : $"({curr}/{goal})";
            var titleText = $"{LocalizeManager.GetText(DataController.Instance.quest.GetQuestType())}\n" +
                            $"{countText}";

            View
                .SetQuestTitle(titleText)
                .SetCountFillAmount(curr, goal)
                .SetQuestLevel((DataController.Instance.quest.currQuestLevel + 1).ToString());
            
            UpdateCanBeCleared();
        }

        private void UpdateCanBeCleared()
        {
            var color = View.EffectImage.color;
            var flag = DataController.Instance.quest.IsCanBeCleared;
            color.a = flag ? 1 : 0;
            View.EffectImage.color = color;
            
            if (flag)
            {
                _effectSequence ??= DOTween.Sequence().SetAutoKill(false)
                    .Append(View.EffectImage.DOFade(0, 0.7f).SetLoops(int.MaxValue, LoopType.Yoyo))
                    .SetUpdate(true);

                if (!_effectSequence.IsPlaying())
                    _effectSequence.Play();
            }
            else
            {
                if (_effectSequence != null)
                {
                    if (_effectSequence.IsPlaying())
                        _effectSequence.Pause();
                }
            }
        }
    }
}