using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataResearch research;
    }

    [Serializable]
    public class DataResearch
    {
        public BResearch[] BDatas => CloudData.CloudData.Instance.bResearches;
        public BInfo BInfoData => CloudData.CloudData.Instance.bInfos[0];

        public Dictionary<ResearchType, UnityAction> OnBindResearch;
        public List<ResearchInfo> researchInfos;
        public List<int> currLevel;

        public string darkDiaChargeTime;

        private Dictionary<ResearchType, List<float>> _values;
        private Dictionary<ResearchType, List<float>> _costs;
        private Dictionary<ResearchType, List<int>> _times;
        private Dictionary<ResearchType, BResearch> _cache;
        private Dictionary<ResearchType, int> _currLevelCache;

        public void InitLevel()
        {
            for (var i = 0; i < currLevel.Count(); ++i)
            {
                currLevel[i] = 0;
            }
        }

        public bool IsResearching(int labSlotIndex)
        {
            var info = researchInfos.FirstOrDefault(x => x.labSlotIndex == labSlotIndex);
            return info is { IsResearching: true };
        }
        
        public bool IsResearching(ResearchType researchType)
        {
            var info = researchInfos.FirstOrDefault(x => x.researchType == researchType);
            return info is { IsResearching: true };
        }

        public ResearchType GetResearchTypeByLabSlotIndex(int labSlotIndex)
        {
            var info = researchInfos.FirstOrDefault(x => x.labSlotIndex == labSlotIndex);
            return info?.researchType ?? ResearchType.None;
        }

        public bool IsResearchTimeComplete(ResearchType researchType)
        {
            var endTime = GetResearchEndTime(researchType);
            return !ServerTime.IsRemainingTimeUntilDisable(endTime);
        }

        public string GetResearchEndTime(ResearchType researchType)
        {
            foreach (var researchInfo in researchInfos)
            {
                if (researchInfo.researchType == researchType)
                    return researchInfo.researchEndTimeToString;
            }

            return string.Empty;
        }

        public float GetDiaForImmediatelyComplete(ResearchType researchType)
        {
            var timeSpan = ServerTime.RemainingTimeToTimeSpan(GetResearchEndTime(researchType));
            var result = Mathf.Max(1, Mathf.CeilToInt(_cache[researchType].diaPerSecond * (float)timeSpan.TotalSeconds));
            return result;
        }
        
        public bool IsMaxLevel(ResearchType researchType)
        {
            return GetMaxLevel(researchType) <= GetCurrLevel(researchType);
        }

        public double GetCost(ResearchType type, int level)
        {
            var cost = _costs.TryGetValue(type, out var costs)
                ? costs[Mathf.Clamp(level, 0, costs.Count - 1)]
                : 0;

            var reduceValue = GetValue(ResearchType.DecreaseResearchCost);
            cost *= 1 - reduceValue;
            
            return cost;
        }

        public void SetLabSlot(int labIndex, ResearchType type, bool isSpecialLab = false)
        {
            var timePerSec =
                DataController.Instance.research.GetResearchTime(type) / GetResearchSpeedReduceValue(isSpecialLab);
            
            researchInfos[labIndex].labSlotIndex = labIndex;
            researchInfos[labIndex].researchType = type;
            researchInfos[labIndex].researchEndTimeToString = ServerTime.DateTimeToIsoString(ServerTime.Date.AddSeconds(timePerSec));
        }

        public float GetResearchSpeedReduceValue(bool isSpecialLab = false)
        {
            return 1 + 
                   (isSpecialLab ? 1 : 0) 
                   + (float)DataController.Instance.good.GetValue(GoodType.IncreaseLabSpeed);
        }

        public void CancelLabSlot(ResearchType type)
        {
            var infoView = researchInfos.FirstOrDefault(x => x.researchType == type);
            infoView?.InitInfo();
        }

        public int GetResearchTime(ResearchType type, int level = -1)
        {
            if (level == -1) level = GetCurrLevel(type);
            
            var time = _times.TryGetValue(type, out var times)
                ? times[Mathf.Clamp(level, 0, times.Count - 1)]
                : 0;
            var reduceValue = GetValue(ResearchType.IncreaseResearchSpeed);
            time = (int)(time * (1 - reduceValue));
            return time;
        }

        public LocalizedTextType GetLocalizedTextType(ResearchType type)
        {
            return _cache.TryGetValue(type, out var data) ? data.localizeType : LocalizedTextType.Research_Title;
        }

        public string GetExpression(ResearchType researchType)
        {
            return _cache.TryGetValue(researchType, out var data) ? data.expression : "";
        }

        public float GetValue(ResearchType type, int level = -1)
        {
            if (level == -1) level = GetCurrLevel(type);
            return _values.TryGetValue(type, out var values)
                ? values[Mathf.Clamp(level, 0, values.Count - 1)]
                : 0;
        }

        public int GetMaxLevel(ResearchType researchType)
        {
            return _cache.TryGetValue(researchType, out var data) ? data.maxLevel : 0;
        }
     
        public int GetSlotIndex(ResearchType researchType)
        {
            return _cache.TryGetValue(researchType, out var data) ? data.slotIndex : 0;
        }

        public int GetCurrLevel(ResearchType type)
        {
            return _currLevelCache.GetValueOrDefault(type, 0);
        }

        public int GetLabSlotIndex(ResearchType researchType)
        {
            var info = researchInfos.FirstOrDefault(x => x.researchType == researchType);
            return info?.labSlotIndex ?? -1;
        }

        public void ResearchComplete(ResearchType researchType)
        {
            LevelUp(researchType);
            InitInfo(researchType);
            
            DataController.Instance.quest.Count(QuestType.AnyResearchCompleate);
            OnBindResearch[researchType]?.Invoke();
        }

        public int GetMaxCreateStorage()
        {
            var value = (int)GetValue(ResearchType.IncreaseCreateStorage);
            return value;
        }

        public int GetMaxSaveStorage()
        {
            var value = (int)GetValue(ResearchType.IncreaseSaveStorage);
            return value;
        }

        public float GetDarkDiaPerSec()
        {
            var value = GetValue(ResearchType.IncreaseDarkDiaPerSec);
            return value;
        }

        public int GetLabCount()
        {
            return BInfoData.labCount;
        }

        public int GetSaveTime()
        {
            return BInfoData.toStorageDelayTime;
        }

        private void LevelUp(ResearchType researchType)
        {
            _currLevelCache[researchType]++;
            var index = GetIndex(researchType);
            currLevel[index]++;
        }

        private void InitInfo(ResearchType researchType)
        {
            var researchInfo = researchInfos.FirstOrDefault(x => x.researchType == researchType);
            researchInfo?.InitInfo();
        }

        private int GetIndex(ResearchType researchType)
        {
            var bData = BDatas.FirstOrDefault(x => x.researchType == researchType);
            return bData?.index ?? 0;
        }
        
        public void Init()
        {
            _cache = new Dictionary<ResearchType, BResearch>();
            _values = new Dictionary<ResearchType, List<float>>();
            _costs = new Dictionary<ResearchType, List<float>>();
            _times = new Dictionary<ResearchType, List<int>>();
            _currLevelCache = new Dictionary<ResearchType, int>();

            OnBindResearch ??= new Dictionary<ResearchType, UnityAction>();
            
            foreach (var data in BDatas)
            {
                _cache.TryAdd(data.researchType, data);
                _values.TryAdd(data.researchType, new List<float>());
                _costs.TryAdd(data.researchType, new List<float>());
                _times.TryAdd(data.researchType, new List<int>());

                _currLevelCache.TryAdd(data.researchType, 0);

                for (var i = 0; i <= data.maxLevel; ++i)
                {
                    _values[data.researchType].Add(data.baseValue + data.increasementValue * i);
                    _costs[data.researchType].Add((data.baseCost + data.increasementCost * i) * (1 + data.multiplierCost));
                    _times[data.researchType].Add(Mathf.CeilToInt((data.baseTime + data.increasementTime * i) * (1 + data.multiplierTime)));
                }

                OnBindResearch.TryAdd(data.researchType, () => { });
            }

            currLevel ??= new List<int>();
            for (var i = 0; i < BDatas.Length; ++i)
            {
                if (currLevel.Count <= i)
                    currLevel.Add(0);
                if (_currLevelCache.ContainsKey(BDatas[i].researchType))
                    _currLevelCache[BDatas[i].researchType] = currLevel[BDatas[i].index];
            }

            researchInfos ??= new List<ResearchInfo>();
            for (var i = researchInfos.Count; i < GetLabCount() + 1; ++i)
            {
                researchInfos.Add(new ResearchInfo());
            }
        }

        public void SetChargeTime(string time)
        {
            darkDiaChargeTime = time;
        }

        public void InitResearchLevel()
        {
            DataController.Instance.good.SetValue(GoodType.SpecialLab, 0);
            for (var i = 0; i < currLevel.Count; ++i)
            {
                currLevel[i] = 0;
                _currLevelCache[(ResearchType)i] = 0;
            }

            foreach (var action in OnBindResearch)
            {
                action.Value?.Invoke();
            }
        }
    }

    [Serializable]
    public class ResearchInfo
    {
        public bool IsResearching => labSlotIndex > -1;
        
        public ResearchType researchType = ResearchType.None;
        public int labSlotIndex = -1;
        public string researchEndTimeToString;

        public void InitInfo()
        {
            researchType = ResearchType.None;
            labSlotIndex = -1;
            researchEndTimeToString = string.Empty;
        }
    }
}