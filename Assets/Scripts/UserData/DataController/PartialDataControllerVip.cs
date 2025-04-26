using System;
using ETD.Scripts.Common;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataVip vip;
    }

    [Serializable]
    public class DataVip
    {
        public bool isGetReward;
        public bool isGetAdsReward;

        public void Init()
        {
            ServerTime.onBindNextDay += OnBindNextDay;
        }

        public bool IsGetRewards()
        {
            return isGetReward && isGetAdsReward;
        }

        public int GetVipRewardCount(int vipLevel)
        {
            return Mathf.Max(vipLevel * 10, 5);
        }

        private void OnBindNextDay()
        {
            isGetReward = false;
            isGetAdsReward = false;
        }
    }
}