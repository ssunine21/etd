using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataAttendance attendance;
    }

    [Serializable]
    public class DataAttendance
    {

        public int Length => BData.goodTypes.Length;
        public bool hasReceivedTodayReward;
        public List<bool> hasReceivedRewards;

        public bool isOpenViewToday;
        
        private BAttendance BData => CloudData.CloudData.Instance.bAttendances[0];

        public void SetOpenViewToday(bool flag)
        {
            isOpenViewToday = flag;
            DataController.Instance.LocalSave();
        }
        
        public GoodType GetGoodType(int index)
        {
            return BData.goodTypes[index];
        }
        
        public float GetValue(int index)
        {
            return BData.values[index];
        }
        
        public int GetParam0(int index)
        {
            return BData.params0[index];
        }

        public bool HasReceivedReward(int index)
        {
            return hasReceivedRewards[index];
        }
        
        public bool CanReceiveReward(int index)
        {
            if (index == 0) return !hasReceivedRewards[index];
            else
            {
                return hasReceivedRewards[index - 1] && !hasReceivedRewards[index] && !hasReceivedTodayReward;
            }
        }

        public bool CanReceiveAny()
        {
            for (var i = 0; i < Length; ++i)
            {
                if (CanReceiveReward(i))
                    return true;
            }

            return false;
        }

        public bool AllReceived()
        {
            return hasReceivedRewards.All(hasReceivedReward => hasReceivedReward);
        }

        public void ReceiveRewardToday(int index)
        {
            hasReceivedRewards[index] = true;
            hasReceivedTodayReward = true;
            
            DataController.Instance.LocalSave();
        }

        public void Init()
        {
            hasReceivedRewards ??= new List<bool>();
            for (var i = hasReceivedRewards.Count; i < Length; ++i)
            {
                hasReceivedRewards.Add(false);
            }
        }
    }
}