using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataFreeGift freeGift;
    }

    [Serializable]
    public class DataFreeGift
    {
        public BFreeGift[] BDatas => CloudData.CloudData.Instance.bFreeGifts;
        public List<int> currPurchasedCounts;

        private Dictionary<int, BFreeGift> _cache;

        public void Init()
        {
            Caching();
        }

        public int GetCurrPurchasedCount(int index)
        {
            return currPurchasedCounts[index];
        }

        public int GetParam(int index)
        {
            return _cache[index].param0;
        }

        public void DiscountCurrPurchased(int index, int count = 1)
        {
            if (currPurchasedCounts.Count <= index) return;
            currPurchasedCounts[index] = 
                Mathf.Clamp(currPurchasedCounts[index] - count, 0, GetMaxPurchaseCount(index));
            
            DataController.Instance.LocalSave();
        }

        public int GetMaxPurchaseCount(int index)
        {
            try
            {
                return _cache[index].maxPurchaseCount;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public GoodType GetRewardType(int index)
        {
            return _cache[index].rewardGoodTypes;
        }

        public double GetRewardValue(int index)
        {
            return _cache[index].rewardValues;
        }
        
        public void OnNextDay()
        {
            for (var i = 0; i < BDatas.Length; ++i)
            {
                if(currPurchasedCounts.Count <= i)
                    currPurchasedCounts.Add(BDatas[i].maxPurchaseCount);

                if (BDatas[i].timeResetType == TimeResetType.Daily)
                    currPurchasedCounts[i] = BDatas[i].maxPurchaseCount;
            }
        }

        private void Caching()
        {
            currPurchasedCounts ??= new List<int>();
            _cache ??= new Dictionary<int, BFreeGift>();
            
            for (var i = 0; i < BDatas.Length; ++i)
            {
                if (currPurchasedCounts.Count <= i)
                    currPurchasedCounts.Add(BDatas[i].maxPurchaseCount);

                _cache.TryAdd(i, BDatas[i]);
            }
        }
    }
}