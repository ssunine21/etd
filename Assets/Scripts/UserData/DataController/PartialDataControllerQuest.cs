using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataQuest quest;
    }

    [Serializable]
    public class DataQuest
    {
        public UnityAction OnBindCount;
        public UnityAction<int> OnBindClear;
        public bool IsCanBeCleared => currCount >= BData[Mathf.Clamp(currQuestLevel, 0, BData.Length - 1)].goalCount;
        public bool IsAllClear => currQuestLevel >= BData.Length;
        public int currCount;
        public int currQuestLevel;

        private int GoalCount => BData[currQuestLevel].goalCount;
        private BQuest[] BData => CloudData.CloudData.Instance.bQuests;

        public KeyValuePair<GoodType, double> GetReward(int currLevel = -1)
        {
            currLevel = NomalizedCurrQuestLevel(currLevel);
            var reward = new KeyValuePair<GoodType, double>(BData[currLevel].rewardGood, BData[currLevel].rewardValue);

            return reward;
        }

        public void TryClear(UnityAction<bool> callback = null)
        {
#if IS_LIVE
            if (currCount < GoalCount)
            {
                callback?.Invoke(false);
                return;
            }
            
#endif

            FirebaseManager.LogEvent("quest", "clear_level", currQuestLevel);
            callback?.Invoke(true);
            

            currQuestLevel++;
            currCount = AsyncCurrCount();
            OnBindClear?.Invoke(currQuestLevel);
        }

        public QuestType GetQuestType(int currLevel = -1)
        {
            currLevel = NomalizedCurrQuestLevel(currLevel);
            return BData[currLevel].questType;
        }

        public int GetGoalCount(int currLevel = -1)
        {
            currLevel = NomalizedCurrQuestLevel(currLevel);
            return BData[currLevel].goalCount;
        }

        public void Count(QuestType type, int count = 1)
        {
            var currLevel = NomalizedCurrQuestLevel(currQuestLevel);
            if (BData[currLevel].questType != type) return;

            currCount = Mathf.Clamp(currCount + count, 0, GoalCount);
            OnBindCount?.Invoke();
        }

        public void SetCount(QuestType type, int count)
        {
            var currLevel = NomalizedCurrQuestLevel(currQuestLevel);
            if (BData[currLevel].questType != type) return;
            currCount = Mathf.Max(currCount, count);
            OnBindCount?.Invoke();
        }

        private int NomalizedCurrQuestLevel(int currLevel)
        {
            return Mathf.Clamp(currLevel < 0 ? currQuestLevel : currLevel, 0, BData.Length - 1);
        }

        private int AsyncCurrCount()
        {
            return GetQuestType() switch
            {
                QuestType.KillEnemy => 0,
                QuestType.SummonElemental => 0,
                QuestType.SummonRune => 0,
                QuestType.ClearNormalStage => DataController.Instance.stage.MaxTotalLevel - 1,
                QuestType.ClearGoldDungeon => 0,
                QuestType.ClearDiaDungeon => 0,
                QuestType.ClearEnhanceDungeon => 0,
                QuestType.UpgradeElementalSlot => DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseElementalUnit),
                QuestType.EquipElemental => 0,
                QuestType.UpgradeRuneSlot => DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseRuneUnit),
                QuestType.EquipRune => 0,
                QuestType.UpgradeProjector => DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseProjector),
                QuestType.EquipElementalWithProjector => 0,
                QuestType.EquipRuneWithProjector => 0,
                QuestType.UpgradeATK => DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseBaseAtkPower),
                QuestType.UpgradeMaxHp => DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseMaxHp),
                QuestType.UpgradeIncreaseHealAmount => DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseHealAmountPerSecond),
                QuestType.EnhanceElemental => 0,
                QuestType.EnhanceRune => 0,
                QuestType.DisassembleRune => 0,
                _ => 0
            };
        }

        public void Init()
        {
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseElementalUnit] += level =>
            {
                SetCount(QuestType.UpgradeElementalSlot, level);
            };
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseRuneUnit] += level =>
            {
                SetCount(QuestType.UpgradeRuneSlot, level);
            };
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseProjector] += level =>
            {
                SetCount(QuestType.UpgradeProjector, level);
            };
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseBaseAtkPower] += level =>
            {
                SetCount(QuestType.UpgradeATK, level);
            };
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseMaxHp] += level =>
            {
                SetCount(QuestType.UpgradeMaxHp, level);
            };
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseHealAmountPerSecond] += level =>
            {
                SetCount(QuestType.UpgradeIncreaseHealAmount, level);
            };

            StageManager.Instance.onBindStageEnd += (stageType, isClear) =>
            {
                if (!isClear) return;
                switch (stageType)
                {
                    case StageType.Normal:
                        break;
                    case StageType.GoldDungeon:
                        DataController.Instance.quest.Count(QuestType.ClearGoldDungeon);
                        break;
                    case StageType.DiaDungeon:
                        DataController.Instance.quest.Count(QuestType.ClearDiaDungeon);
                        break;
                    default:
                        break;
                }
            };
            
            StageManager.Instance.onBindStageClear += stageType =>
            {
                switch (stageType)
                {
                    case StageType.EnhanceDungeon:
                        DataController.Instance.quest.Count(QuestType.ClearEnhanceDungeon);
                        break;
                    default:
                        break;
                }
            }; 

            
            StageManager.Instance.onBindStartStage += (stageType, level) =>
            {
                if (stageType == StageType.Normal)
                    DataController.Instance.quest.SetCount(QuestType.ClearNormalStage, level - 1);
            };
        }
    }
}