using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataGrowPass growPass;
    }

    [Serializable]
    public class DataGrowPass
    {
        public List<GrowPassEarnData> EarnDatas;
        
        private BGrowPass[] BGrowPasses => CloudData.CloudData.Instance.bGrowPasses;
        private Dictionary<PassType, Dictionary<int, List<BGrowPass>>> _cache;

        public int FineLevelByGoal(PassType passType, int goal)
        {
            var level = GetLevelCount(passType);
            for (var i = 0; i < level; ++i)
            {
                if (_cache[passType][i][^1].goal >= goal)
                    return i;
            }

            return -1;
        }

        public GoodType GetRewardType(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            var cache = Get(passType, level, index);
            return battlePassType == BattlePassType.Premium ? cache.pRewardGoodType : cache.nRewardGoodType;
        }

        public double GetLastRewardValue(PassType passType, int level)
        {
            var lastIndex = GetGoalCount(passType, level);
            return GetRewardValue(passType, level, lastIndex - 1, BattlePassType.Premium);
        }
        
        public double GetRewardValue(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            var cache = Get(passType, level, index);
            return battlePassType == BattlePassType.Premium ? cache.pRewardValue : cache.nRewardValue;
        }
        
        public int GetRewardParam0(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            var cache = Get(passType, level, index);
            return battlePassType == BattlePassType.Premium ? cache.pRewardParam0 : cache.nRewardParam0;
        }

        public bool HasReward(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            var earnData = GetEarnData(passType);
            return earnData.HasReward(level, index, battlePassType);
        }

        public bool CanReceiveReward(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            var earnData = GetEarnData(passType);
            return earnData.CanReceiveReward(level, index, battlePassType) && !IsLock(passType, level, index, battlePassType);
        }

        public bool HasPass(PassType passType, int level)
        {
            var earnData = GetEarnData(passType);
            return earnData.HasPass(level);
        }
        
        public BGrowPass Get(PassType passType, int level, int index)
        {
            return _cache[passType][level][index];
        }

        public GrowPassEarnData GetEarnData(PassType passType)
        {
            return EarnDatas[(int)passType];
        }

        public bool IsLock(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            var curr = passType switch
            {
                PassType.StagePass => DataController.Instance.stage.MaxTotalLevel,
                PassType.ElementalPass => DataController.Instance.elemental.summonCount,
                PassType.RunePass => DataController.Instance.rune.summonCount,
                _ => 0
            };
            var goal = _cache[passType][level][index].goal;
            
            if (battlePassType == BattlePassType.Normal)
                return curr < goal;
            
            return !HasPass(passType, level) || curr < goal;
        }

        public int GetGoal(PassType passType, int level, int index)
        {
            return _cache[passType][level][index].goal;
        }
        
        public int GetLevelCount(PassType passType)
        {
            return _cache[passType].Count;
        }

        public int GetGoalCount(PassType passType, int level)
        {
            return _cache[passType][level].Count;
        }

        public int GetGoalCountUntilLevel(PassType passType, int level)
        {
            var total = 0;
            for (var i = 0; i < level; ++i)
            {
                total += GetGoalCount(passType, i);
            }

            return total;
        }

        public int GetAllGoalCount(PassType passType)
        {
            var level = GetLevelCount(passType);
            return GetGoalCountUntilLevel(passType, level);
        }

        public void SetReceive(PassType passType, int level, int index, BattlePassType battlePassType)
        {
            EarnDatas[(int)passType].SetReceived(level, index, battlePassType);
        }

        public void SetPass(PassType passType, int level)
        {
            EarnDatas[(int)passType].SetPass(level);
        }
        
        public void Init()
        {
            _cache = new Dictionary<PassType, Dictionary<int, List<BGrowPass>>>();
            foreach (var bGrowPass in BGrowPasses)
            {
                _cache.TryAdd(bGrowPass.passType, new Dictionary<int, List<BGrowPass>>());
                if(_cache.ContainsKey(bGrowPass.passType))
                {
                    _cache[bGrowPass.passType].TryAdd(bGrowPass.level, new List<BGrowPass>());
                    if (_cache[bGrowPass.passType].ContainsKey(bGrowPass.level))
                    {
                        _cache[bGrowPass.passType][bGrowPass.level].Add(bGrowPass);
                    }
                }
            }

            EarnDatas ??= new List<GrowPassEarnData>();
            foreach (PassType passType in Enum.GetValues(typeof(PassType)))
            {
                if(EarnDatas.Count <= (int)passType)
                    EarnDatas.Add(new GrowPassEarnData(passType));
                EarnDatas[(int)passType].Init();
            }
        }
    }

    [Serializable]
    public class GrowPassEarnData
    {
        public PassType passType;
        public List<bool> hasNReward;
        public List<bool> hasPReward;
        public List<bool> hasPass;

        private Dictionary<int, List<bool>> _hasNRewardCache;
        private Dictionary<int, List<bool>> _hasPRewardCache;

        public GrowPassEarnData(PassType passType)
        {
            this.passType = passType;
        }

        public void Init()
        {
            _hasNRewardCache = new Dictionary<int, List<bool>>();
            _hasPRewardCache = new Dictionary<int, List<bool>>();
            
            var levelCount = DataController.Instance.growPass.GetLevelCount(passType);
            var allCount = DataController.Instance.growPass.GetAllGoalCount(passType);
            
            hasNReward ??= new List<bool>();
            hasPReward ??= new List<bool>();
            hasPass ??= new List<bool>();
            for (var i = hasNReward.Count; i < allCount; ++i)
            {
                hasNReward.Add(false);
                hasPReward.Add(false);
                hasPass.Add(false);
            }
            
            var currGoalCount = 0;
            for (var i = 0; i < levelCount; ++i)
            {
                var goalCount = DataController.Instance.growPass.GetGoalCount(passType, i);
                _hasNRewardCache.TryAdd(i, hasNReward.Skip(currGoalCount).Take(goalCount).ToList());
                _hasPRewardCache.TryAdd(i, hasPReward.Skip(currGoalCount).Take(goalCount).ToList());
                currGoalCount += goalCount;
            }
        }

        public bool HasPass(int level)
        {
            return hasPass.Count > level && hasPass[level];
        }

        public void SetPass(int level)
        {
            if (hasPass.Count > level)
                hasPass[level] = true;
        }

        public void SetReceived(int level, int index, BattlePassType battlePassType)
        {
            var idx = GetIndex(level, index);
            if (battlePassType == BattlePassType.Normal)
            {
                _hasNRewardCache[level][index] = true;
                hasNReward[idx] = true;
            }
            else
            {
                _hasPRewardCache[level][index] = true;
                hasPReward[idx] = true;
            }
        }
        
        public bool HasReward(int level, int index, BattlePassType battlePassType)
        {
            return battlePassType == BattlePassType.Premium ? _hasPRewardCache[level][index] : _hasNRewardCache[level][index];
        }

        public bool CanReceiveReward(int level, int index, BattlePassType battlePassType)
        {
            return battlePassType == BattlePassType.Premium ? !_hasPRewardCache[level][index] : !_hasNRewardCache[level][index];
        }

        public int GetIndex(int level, int index)
        {
            var idx = DataController.Instance.growPass.GetGoalCountUntilLevel(passType, level) + index;
            return idx;
        }
    }
}