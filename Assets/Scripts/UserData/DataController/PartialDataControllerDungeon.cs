using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.CloudData;
using JetBrains.Annotations;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataDungeon dungeon;
    }

    [Serializable]
    public class DataDungeon
    {
        public List<double> lastEarnedRewards;
        public List<double> maxGoalCounts;

        public List<int> adsEnterCounts;
        
        public Dictionary<GoodType, double> currStageEachEnemyRewardDictionary;
        
        private BDungeon[] BDatas => CloudData.CloudData.Instance.bDungeons;
        private Dictionary<StageType, List<BDungeon>> _cache;

        private const int MaxFreeTicketCount = 2;
        private const int MaxAdsCount = 2;

        public void Init()
        {
            Caching();
            StageManager.Instance.onBindStartStage += AsyncStageReward;
            ServerTime.onBindNextDay += OnBindNextDay;
        }

        public float GetReduceValue()
        {
            return 1
                   + DataController.Instance.research.GetValue(ResearchType.IncreaseDungeonReward)
                   + (float)DataController.Instance.good.GetValue(GoodType.IncreaseDungeonReward);
        }
        
        public EnemyDifficultyInfo GetEnemyDifficultyInfo(StageType stageType, int level)
        {
            if (TryGetCaching(stageType, level, out var cacheData))
            {
                var info = new EnemyDifficultyInfo(cacheData.enemyCombinationIndex
                    , cacheData.difficultyLevel, cacheData.incrementDifficultyValue);
                return info;
            }

            return new EnemyDifficultyInfo();
        }

        public void SetMaxGoalCount(StageType type, double count)
        {
            if (type == StageType.Normal) return;
            maxGoalCounts[(int)type] = Math.Max(count, maxGoalCounts[(int)type]);
        }

        public void SetLastEarnRewards(IEnumerable<ViewGood> viewGoods )
        {
            foreach (var viewGood in viewGoods)
            {
                if(viewGood.GoodType == GoodType.None) return;
                var goodTypeIndex = (int)viewGood.GoodType;
                if (goodTypeIndex < 0 || goodTypeIndex >= lastEarnedRewards.Count) return;

                lastEarnedRewards[goodTypeIndex] = Math.Max(viewGood.GoodValue, lastEarnedRewards[goodTypeIndex]);
            }
        }

        public Dictionary<GoodType, double> GetLastEarnedRewards(StageType type)
        {
            var results = new Dictionary<GoodType, double>();

            if (TryGetCaching(type, 0, out var cacheData))
            {
                foreach (var rewardType in cacheData.rewardTypes)
                {
                    if(rewardType == GoodType.None) continue;
                    results.TryAdd(rewardType, lastEarnedRewards[(int)rewardType]);
                }
            }
            
            return results;
        }

        public int GetNextLevel(StageType type, int currLevel)
        {
            return Mathf.Clamp(currLevel + 1, 0, _cache[type].Count - 1);
        }

        public float GetTimeLimit(StageType type, int level)
        {
            if (TryGetCaching(type, level, out var cacheData))
            {
                return cacheData.timeLimit;
            }

            return 0;
        }

        public double GetMaxGoalCount(StageType type)
        {
            var index = (int)type;
            return maxGoalCounts[Mathf.Clamp(index, 0, maxGoalCounts.Count - 1)];
        }

        public int GetAdsEnterCount(StageType type)
        {
            return adsEnterCounts[(int)type];
        }

        public GoodType GetTicketGoodType(StageType type)
        {
            return type switch
            {
                StageType.GoldDungeon => GoodType.GoldDungeonTicket,
                StageType.DiaDungeon => GoodType.GemDungeonTicket,
                StageType.EnhanceDungeon => GoodType.EnhanceDungeonTicket,
                StageType.DarkDiaDungeon => GoodType.RaidTicket,
                StageType.GuildRaidDungeon => GoodType.GuildRaidTicket,
                _ => GoodType.None
            };
        }

        public int GetEnemyCombinationIndex(StageType stageType, int level)
        {
            return TryGetCaching(stageType, level, out var cacheData) ? cacheData.enemyCombinationIndex : 0;
        }

        public void ConsumeAdsCount(StageType stageType, int count = 1)
        {
            adsEnterCounts[(int)stageType] -= count;
        }
            
        private bool TryGetCaching(StageType type, int level, out BDungeon cacheData)
        {
            if(_cache.ContainsKey(type))
                if (_cache[type].Count > level)
                {
                    cacheData = _cache[type][level];
                    return true;
                }

            cacheData = null;
            return false;
        }

        private void OnBindNextDay()
        {
            for (var i = 0; i < adsEnterCounts.Count; ++i)
            {
                adsEnterCounts[i] = MaxAdsCount;
            }
            
            DataController.Instance.good.SetValue(GoodType.GoldDungeonTicket,
                Math.Max(DataController.Instance.good.GetValue(GoodType.GoldDungeonTicket), MaxFreeTicketCount));
            DataController.Instance.good.SetValue(GoodType.GemDungeonTicket,
                Math.Max(DataController.Instance.good.GetValue(GoodType.GemDungeonTicket), MaxFreeTicketCount));
            DataController.Instance.good.SetValue(GoodType.EnhanceDungeonTicket,
                Math.Max(DataController.Instance.good.GetValue(GoodType.EnhanceDungeonTicket), MaxFreeTicketCount));
        }

        private void Caching()
        {
            _cache ??= new Dictionary<StageType, List<BDungeon>>();
            maxGoalCounts ??= new List<double>();
            lastEarnedRewards ??= new List<double>();
            adsEnterCounts ??= new List<int>();
            
            foreach (var bData in BDatas)
            {
                _cache.TryAdd(bData.stageType, new List<BDungeon>());
                _cache[bData.stageType].Add(bData);
            }

            for (var i = 0; i < Enum.GetValues(typeof(StageType)).Length; ++i)
            {
                if (maxGoalCounts.Count <= i)
                    maxGoalCounts.Add(0);
                
                if(adsEnterCounts.Count <= i)
                    adsEnterCounts.Add(MaxAdsCount);
            }
            
            for (var i = 0; i < Enum.GetValues(typeof(GoodType)).Length; ++i)
            {
                if (lastEarnedRewards.Count <= i)
                    lastEarnedRewards.Add(0);
            }
        }
        
        private void AsyncStageReward(StageType stageType, int level)
        {
            if (stageType == StageType.Normal) return;
            currStageEachEnemyRewardDictionary ??= new Dictionary<GoodType, double>();
            currStageEachEnemyRewardDictionary.Clear();
            
            if(TryGetCaching(stageType, level, out var cacheData))
            {
                currStageEachEnemyRewardDictionary = cacheData.rewardTypes.ToDictionary(
                    key => key,
                    key => cacheData.rewardValues[
                               cacheData.rewardTypes.ToList().IndexOf(key)] /
                           DataController.Instance.enemyCombination.GetEnemyCount(
                               GetEnemyCombinationIndex(stageType, level)));
            }
        }

        [CanBeNull]
        public List<GoodItem> GetDungeonReward(StageType stageType, int level)
        {
            List<GoodItem> goodItems = null;
            if (TryGetCaching(stageType, level, out var cacheData))
            {
                goodItems = cacheData.rewardTypes.Select((t, i) => new GoodItem(t, cacheData.rewardValues[i])).ToList();
            }

            return goodItems;
        }
    }
}