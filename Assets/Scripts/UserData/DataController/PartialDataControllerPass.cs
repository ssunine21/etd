using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataPass pass;
    }

    [Serializable]
    public class DataPass
    {
        public int BLength => BPasses.Length;
        public int currTotalExp = 1000;
        public bool isPremium;

        public int nElementalParam0;
        public int pElementalParam0;

        public List<bool> hasNReward;
        public List<bool> hasPReward;

        public UnityAction OnBindChangeExp;

        public string remainTimeToString;
        public bool IsMaxLevel => GetLevel() >= BLength;
        
        private BPass[] BPasses => CloudData.CloudData.Instance.bPasses;

        public void ResetData()
        {
            currTotalExp = 1000;
            isPremium = false;

            for (var i = 0; i < hasNReward.Count; ++i) hasNReward[i] = false;
            for (var i = 0; i < hasPReward.Count; ++i) hasPReward[i] = false;

            var nextTime = ServerTime.Date.AddDays(28);
            SetRemainTimeToString(ServerTime.DateTimeToIsoString(new DateTime(nextTime.Year, nextTime.Month, nextTime.Day)));
            DataController.Instance.SaveBackendData();
        }

        public bool CanReceiveReward(int index, BattlePassType type)
        {
            var currLevel = GetLevel();
            return type switch
            {
                BattlePassType.Normal => !hasNReward[index] && currLevel > index,
                BattlePassType.Premium => !hasPReward[index] && currLevel > index && isPremium,
                _ => false
            };
        }

        public bool IsPremium()
        {
            return isPremium;
        }

        public void SetPremium(bool flag)
        {
            isPremium = flag;
            DataController.Instance.LocalSave();
        }

        public string GetRemainTimeToString()
        {
            return remainTimeToString;
        }

        public int GetIncreaseExpAmount(TimeResetType missionType)
        {
            return missionType switch
            {
                TimeResetType.Daily => BPasses[0].increasePointAmount[0],
                TimeResetType.Weekly => BPasses[0].increasePointAmount[1],
                TimeResetType.Repeat => BPasses[0].increasePointAmount[2],
                _ => 0
            };
        }

        public void SetRemainTimeToString(string time)
        {
            remainTimeToString = time;
            DataController.Instance.LocalSave();
        }

        public void HasReceived(int index, BattlePassType battlePassType)
        {
            if (battlePassType == BattlePassType.Normal) hasNReward[index] = true;
            else hasPReward[index] = true;
            DataController.Instance.LocalSave();
        }

        public bool HasReceivedReward(int index, BattlePassType type)
        {
            return type switch
            {
                BattlePassType.Normal => !hasNReward[index],
                BattlePassType.Premium => !hasPReward[index],
                _ => false
            };
        }
        
        public GoodType GetRewardGoodType(int index, BattlePassType battlePassType)
        {
            return battlePassType == BattlePassType.Normal
                ? BPasses[index].nRewardGoodType
                : BPasses[index].pRewardGoodType;
        }

        public double GetRewardValue(int index, BattlePassType battlePassType)
        {
            return battlePassType == BattlePassType.Normal
                ? BPasses[index].nRewardValue
                : BPasses[index].pRewardValue;
        }

        public int GetRewardParam0(int index, BattlePassType battlePassType)
        {
            if(index == 0)
            {
                return battlePassType == BattlePassType.Normal
                    ? nElementalParam0
                    : pElementalParam0;
            }
            else
            {
                return battlePassType == BattlePassType.Normal
                    ? BPasses[index].nRewardParam0
                    : BPasses[index].pRewardParam0;
            }
        }

        public void SetMasterLevel()
        {
            var totalExp = BPasses.Sum(x => x.exp);
            currTotalExp = totalExp;
            
            DataController.Instance.LocalSave();
        }

        public int GetMaxExp(int index)
        {
            return BPasses[Mathf.Clamp(index, 0, BPasses.Length - 1)].exp;
        }

        public int GetSelectedElementalIndex(BattlePassType battleType)
        {
            return battleType == BattlePassType.Normal ? nElementalParam0 : pElementalParam0;
        }

        public void SetSelectedElementalIndex(int elementalParam, BattlePassType battleType)
        {
            if (battleType == BattlePassType.Normal) nElementalParam0 = elementalParam;
            else pElementalParam0 = elementalParam;
            
            DataController.Instance.LocalSave();
        }

        public void IncreaseExp(TimeResetType timeResetType)
        {
            var point = DataController.Instance.pass.GetPoint(timeResetType);
            currTotalExp += point;
            
            OnBindChangeExp?.Invoke();
            DataController.Instance.LocalSave();
        }

        public int GetLevel()
        {
            var level = 0;
            var tempExp = currTotalExp;
            
            foreach (var bPass in BPasses)
            {
                tempExp -= bPass.exp;
                if (tempExp < 0)
                    return level;
                ++level;
            }

            return level;
        }

        public int GetRemainExp()
        {
            var tempExp = currTotalExp;
            foreach (var bPass in BPasses)
            {
                if (tempExp - bPass.exp < 0)
                    return tempExp;
                
                tempExp -= bPass.exp;
            }

            return tempExp;
        }

        public int GetPoint(TimeResetType timeResetType)
        {
            if (IsMaxLevel) return 0;
            
            return timeResetType switch
            {
                TimeResetType.Daily => BPasses[GetLevel()].increasePointAmount[0],
                TimeResetType.Weekly => BPasses[GetLevel()].increasePointAmount[1],
                TimeResetType.Repeat => BPasses[GetLevel()].increasePointAmount[2],
                _ => 0
            };
        }

        public void Init()
        {
            hasNReward ??= new List<bool>();
            hasPReward ??= new List<bool>();

            for (var i = hasNReward.Count; i < BLength; ++i) hasNReward.Add(false);
            for (var i = hasPReward.Count; i < BLength; ++i) hasPReward.Add(false);

            if (nElementalParam0 == 0)
                nElementalParam0 = BPasses[0].nRewardParam0;
            if (pElementalParam0 == 0)
                pElementalParam0 = BPasses[0].pRewardParam0;
        }
    }
}