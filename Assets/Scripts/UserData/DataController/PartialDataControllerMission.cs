using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using JetBrains.Annotations;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataMission mission;
    }

    [Serializable]
    public class DataMission
    {
        public UnityAction<MissionType> onBindMissionCount;
        public UnityAction<TimeResetType, MissionType, bool> onBindGetRewarded;     
        public BMission[] BDatas => CloudData.CloudData.Instance.bMissions;


        public List<Mission> dailyMissions;
        public List<Mission> weeklyMissions;
        public List<Mission> repeatMissions;

        private Dictionary<TimeResetType, Dictionary<MissionType, Mission>> _localMissionDataCache;
        
        private Dictionary<TimeResetType, Dictionary<MissionType, BMission>> _cache;
        private Dictionary<TimeResetType, Dictionary<MissionType, BMission>> _rewardCache;
        private Dictionary<TimeResetType, Dictionary<MissionType, BMission>> _totalRewardMissionTypes;

        public void Init()
        {
            Caching();
            
            
            StageManager.Instance.onBindStartStage += (stageType, level) =>
            {
                if (stageType == StageType.Normal)
                    DataController.Instance.mission.Count(MissionType.ClearNormalStage);
            };
            
            
            StageManager.Instance.onBindStageEnd += (stageType, isClear) =>
            {
                if (!isClear) return;
                switch (stageType)
                {
                    case StageType.GoldDungeon:
                        DataController.Instance.mission.Count(MissionType.ClearGoldDungeon);
                        DataController.Instance.mission.Count(MissionType.ClearAnyDungeon);
                        break;
                    case StageType.DiaDungeon:
                        DataController.Instance.mission.Count(MissionType.ClearDiaDungeon);
                        DataController.Instance.mission.Count(MissionType.ClearAnyDungeon);
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
                        DataController.Instance.mission.Count(MissionType.ClearEnhanceDungeon);
                        DataController.Instance.mission.Count(MissionType.ClearAnyDungeon);
                        break;
                    default:
                        break;
                }
            }; 
            
            

            ServerTime.onBindNextDay += OnNextDay;
            ServerTime.onBindNextWeek += OnNextWeek;
        }
        
        public KeyValuePair<GoodType, double> GetGoodReward(TimeResetType resetType, MissionType missionType, bool isAds)
        {
            var key = GoodType.Gold;
            var value = 0d;
            var cache = GetCache(resetType, missionType);
            
            if (cache != null)
            {
                if (resetType == TimeResetType.Repeat)
                {
                    var multiple = GetCurrCount(resetType, missionType) / GetGoalCount(resetType, missionType);
                    key = cache.rewardGoodTypes[0];
                    value = cache.rewardGoodValues[0] * multiple;
                }
                else
                {
                    key = isAds ? cache.rewardGoodTypes[1] : cache.rewardGoodTypes[0];
                    value = isAds ? cache.rewardGoodValues[1] : cache.rewardGoodValues[0];
                }
            }
            
            var keyValue = new KeyValuePair<GoodType, double>(key, value);
            return keyValue;
        }

        public bool CanEarned(TimeResetType resetType, MissionType missionType, bool isAds)
        {
            var isEarned = IsGetReward(resetType, missionType, isAds);
            return GetCurrCount(resetType, missionType) >= GetGoalCount(resetType, missionType) && !isEarned;
        }

        public bool IsGetReward(TimeResetType resetType, MissionType missionType, bool isAds)
        {
            if(_localMissionDataCache.TryGetValue(resetType, out var value0))
                if (value0.TryGetValue(missionType, out var value1))
                    return isAds ? value1.isAdsRewardEarned : value1.isRewardEarned;
            return false;
        }

        public int GetClearCount(TimeResetType resetType)
        {
            var result = 0;
            result = _localMissionDataCache[resetType].Count(x =>
                x.Value.isRewardEarned && !IsTotalMission(x.Value.missionType));
            return result;
        }
        
        public int GetMissionCount(TimeResetType resetType)
        {
            return _rewardCache.TryGetValue(resetType, out var values) ? values.Count : 0;
        }
        
        public BMission GetCache(TimeResetType resetType, MissionType missionType)
        {
            if (_cache.TryGetValue(resetType, out var value0))
                if (value0.TryGetValue(missionType, out var value1))
                    return value1;
            return null;
        }
        
        [CanBeNull]
        public BMission GetRewardCache(TimeResetType resetType, MissionType missionType)
        {
            if (_rewardCache.TryGetValue(resetType, out var value0))
                if (value0.TryGetValue(missionType, out var value1))
                    return value1;
            return null;
        }
        
        public Dictionary<MissionType, BMission> GetRewardCaches(TimeResetType resetType)
        {
            return _rewardCache.GetValueOrDefault(resetType);
        }

        public Dictionary<MissionType, BMission> GetTotalRewardCaches(TimeResetType resetType)
        {
            return _totalRewardMissionTypes.GetValueOrDefault(resetType);
        }
        
        public int GetCurrCount(TimeResetType resetType, MissionType missionType)
        {
            if(_localMissionDataCache.TryGetValue(resetType, out var value0))
                if (value0.TryGetValue(missionType, out var value1))
                    return value1.currCount;
            return 0;
        }

        public int GetGoalCount(TimeResetType resetType, MissionType missionType)
        {
            var cache = GetCache(resetType, missionType);
            return cache?.goalCount ?? 0;
        }

        public void Count(MissionType missionType, int count = 1)
        {
            foreach (var localCaches in _localMissionDataCache.Values)
            {
                if (localCaches.TryGetValue(missionType, out var value))
                    value.currCount += count;
            }
            onBindMissionCount?.Invoke(missionType);
        }

        public void CountTotalMission(TimeResetType resetType)
        {
            if (resetType == TimeResetType.Repeat) return;
            foreach (var totalBdata in GetTotalRewardCaches(resetType).Values)
            {
                Count(totalBdata.missionType);
            }
        }
        
        public void SetIsRewardEarned(TimeResetType resetType, MissionType missionType, bool isAds, bool flag)
        {
            if(_localMissionDataCache.TryGetValue(resetType, out var value0))
                if (value0.TryGetValue(missionType, out var value1))
                {
                    if (resetType == TimeResetType.Repeat)
                    {
                        value1.currCount %= GetGoalCount(resetType, missionType);
                    }
                    else
                    {
                        if (isAds) value1.isAdsRewardEarned = flag;
                        else value1.isRewardEarned = flag;
                    }
                }
        }

        public bool IsTotalMission(MissionType missionType)
        {
            return missionType.ToString().Contains("DailyMission") || missionType.ToString().Contains("WeeklyMission");
        }

        private void Caching()
        {
            _cache ??= new Dictionary<TimeResetType, Dictionary<MissionType, BMission>>();
            _rewardCache ??= new Dictionary<TimeResetType, Dictionary<MissionType, BMission>>();
            _totalRewardMissionTypes ??= new Dictionary<TimeResetType, Dictionary<MissionType, BMission>>();
            _localMissionDataCache ??= new Dictionary<TimeResetType, Dictionary<MissionType, Mission>>();

            dailyMissions ??= new List<Mission>();
            weeklyMissions ??= new List<Mission>();
            repeatMissions ??= new List<Mission>();

            foreach (var bData in BDatas)
            {

                if (!dailyMissions.Exists(x => x.missionType == bData.missionType))
                {
                    dailyMissions.Add(new Mission(bData.missionType));
                    weeklyMissions.Add(new Mission(bData.missionType));
                    repeatMissions.Add(new Mission(bData.missionType));
                }
                
                if(IsTotalMission(bData.missionType))
                {
                    _totalRewardMissionTypes.TryAdd(bData.resetType, new Dictionary<MissionType, BMission>());
                    _totalRewardMissionTypes[bData.resetType].TryAdd(bData.missionType, bData);
                }
                else
                {
                    _rewardCache.TryAdd(bData.resetType, new Dictionary<MissionType, BMission>());
                    _rewardCache[bData.resetType].TryAdd(bData.missionType, bData);
                }
                
                _cache.TryAdd(bData.resetType, new Dictionary<MissionType, BMission>());
                _cache[bData.resetType].TryAdd(bData.missionType, bData);
            }

            _localMissionDataCache.Add(TimeResetType.Daily, new Dictionary<MissionType, Mission>());
            _localMissionDataCache.Add(TimeResetType.Weekly, new Dictionary<MissionType, Mission>());
            _localMissionDataCache.Add(TimeResetType.Repeat, new Dictionary<MissionType, Mission>());

            for (var i = 0; i < dailyMissions.Count; ++i)
            {
                var dailyType = dailyMissions[i].missionType;
                var weeklyType = weeklyMissions[i].missionType;
                var repeatType = repeatMissions[i].missionType;

                _localMissionDataCache[TimeResetType.Daily].TryAdd(dailyType, dailyMissions[i]);
                _localMissionDataCache[TimeResetType.Weekly].TryAdd(weeklyType, weeklyMissions[i]);
                _localMissionDataCache[TimeResetType.Repeat].TryAdd(repeatType, repeatMissions[i]);
            }
        }

        private void OnNextDay()
        {
            foreach (var dailyData in _localMissionDataCache[TimeResetType.Daily].Values)
            {
                dailyData.Init();
            }
            Count(MissionType.DailyLogin);
        }

        private void OnNextWeek()
        {
            foreach (var weeklyData in _localMissionDataCache[TimeResetType.Weekly].Values)
            {
                weeklyData.Init();
            }
            _localMissionDataCache[TimeResetType.Weekly][MissionType.DailyLogin].currCount = 1;
        }
    }

    [Serializable]
    public class Mission
    {
        public MissionType missionType;
        public int currCount;
        public bool isRewardEarned;
        public bool isAdsRewardEarned;
        
        public Mission() { }

        public Mission(MissionType type)
        {
            missionType = type;
            currCount = 0;
            isRewardEarned = false;
            isAdsRewardEarned = false;
        }

        public void Init()
        {
            currCount = 0;
            isRewardEarned = false;
            isAdsRewardEarned = false;
        }
    }
}