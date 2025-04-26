using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataOfflineReward offlineReward;
    }

    [Serializable]
    public class DataOfflineReward
    {
        private BOfflineReward[] BDatas => CloudData.CloudData.Instance.bOfflineRewards;

        private string _updateDateToString;
        private Dictionary<GoodType, BOfflineReward> _cache;

        public void Init()
        {
            _updateDateToString = DataController.Instance.setting.updateAt;

            _cache = new Dictionary<GoodType, BOfflineReward>();
            foreach (var bData in BDatas)
            {
                _cache.TryAdd(bData.goodType, bData);
            }
        }
        
        public TimeSpan GetOfflineTimeSpan()
        {
            if (!ServerTime.IsInit) return TimeSpan.Zero;

            var updateAt = string.IsNullOrEmpty(_updateDateToString)
                ? ServerTime.Date
                : ServerTime.IsoStringToDateTime(_updateDateToString);
            
            var timeSpan = ServerTime.UntilTimeToServerTime(updateAt);
            return timeSpan;
        }

        public int GetTotalTime(GoodType goodType)
        {
            return _cache.TryGetValue(goodType, out var value) ? value.totalTimeHour : 0;
        }

        public List<GoodItem> GetRewardGoodItems()
        {
            var stageIndex = DataController.Instance.stage.MaxTotalLevel / DataController.Instance.stage.StageSpacing;
            var rewardTypes = DataController.Instance.stage.BDatas[stageIndex].rewardTypes;
            var newGoodItems = new List<GoodItem>();
            
            foreach (var rewardType in rewardTypes)
            {
                var rewardValue = GetValueWhileOffline(rewardType);
                if(rewardValue < 1) continue;
                
                newGoodItems.Add(new GoodItem(rewardType, rewardValue));
            }

            return newGoodItems;
        }

        public double GetValueWhileOffline(GoodType goodType)
        {
            var timeSpan = GetOfflineTimeSpan();
            var totalTime = GetTotalTime(goodType);
            var hours = Mathf.Min(totalTime, (int)timeSpan.TotalHours);
            var minutes = hours >= totalTime
                ? 0
                : (int)timeSpan.TotalMinutes - (int)timeSpan.TotalHours * 60;

            var totalValue = GetValueUntilTime(goodType, hours, minutes);
            totalValue *=
                1 + DataController.Instance.research.GetValue(ResearchType.IncreaseOfflineReward);
            
            return totalValue;
        }

        public double GetValueUntilTime(GoodType goodType, float hour, int minute)
        {
            if (_cache.TryGetValue(goodType, out var value))
            {
                var stageIndex = DataController.Instance.stage.MaxTotalLevel / DataController.Instance.stage.StageSpacing;
                var rewardTypes = DataController.Instance.stage.BDatas[stageIndex].rewardTypes;
                var rewardValues = DataController.Instance.stage.BDatas[stageIndex].rewardValues;

                for (var i = 0; i < rewardTypes.Length; ++i)
                {
                    if (rewardTypes[i] == goodType)
                    {
                        return rewardValues[i] * value.valuePerMinute * (hour * 60 + minute);
                    }
                }
            }

            return 0;
        }
    }
}