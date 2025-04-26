using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using JetBrains.Annotations;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataGuildReward guildReward;
    }

    [Serializable]
    public class DataGuildReward
    {
        public bool IsDonationable => donationCount < GetMaxDonationCount();
        public int ShopProductCount => shopPurchaseCounts.Count;
        public int donationCount;
        public List<int> shopPurchaseCounts;

        private Dictionary<GuildRewardType, Dictionary<int, BGuildReward>> _cache = new();
        private BInfo BInfoData => CloudData.CloudData.Instance.bInfos[0];
        
        public void Init()
        {
            shopPurchaseCounts ??= new List<int>();
            for (var i = shopPurchaseCounts.Count; i < CloudData.CloudData.Instance.bGuildRewards.Count(x => x.guildRewardType == GuildRewardType.Shop); ++i)
                shopPurchaseCounts.Add(0);
            
            foreach (var guildReward in CloudData.CloudData.Instance.bGuildRewards)
            {
                if (!_cache.ContainsKey(guildReward.guildRewardType))
                    _cache[guildReward.guildRewardType] = new Dictionary<int, BGuildReward>();

                if (!_cache[guildReward.guildRewardType].ContainsKey(guildReward.step))
                    _cache[guildReward.guildRewardType][guildReward.step] = guildReward;
            }
        }

        public int GetGiftBoxPointWithRaid()
        {
            return BInfoData.guildGiftBoxPointWithRaid;
        }

        [CanBeNull]
        public BGuildReward GetCache(GuildRewardType guildRewardType, int step)
        {
            if(_cache.TryGetValue(guildRewardType, out var value0))
                if (value0.TryGetValue(step, out var value1))
                    return value1;
            return null;
        }
        
        public List<BGuildReward> GetCaches(GuildRewardType guildRewardType)
        {
            return _cache.TryGetValue(guildRewardType, out var value) ? value.Values.ToList() : new List<BGuildReward>();
        }
        
        public int GetMaxDonationCount()
        {
            return _cache.TryGetValue(GuildRewardType.Donation, out var rewards) ? rewards.Count : 0;
        }

        public GoodType[] GetRewardGoodTypes(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.rewardTypes;
        }
        
        public double[] GetRewardValues(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.rewardValues;
        }
        
        public int[] GetRewardParams(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.rewardParams;
        }
        
        public GoodType GetNeedGoodTypes(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.needGoodType ?? GoodType.None;
        } 
        
        public int GetNeedValue(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.needValue ?? 0;
        }

        public LocalizedTextType GetLocalizeTextType(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.localizeTextType ?? LocalizedTextType.Guild_GiftBox_Normal0;
        }

        public int GetMaxGiftBoxExp(int step)
        {
            return GetCache(GuildRewardType.GiftBox, step)?.exp ?? 0;
        }

        [CanBeNull]
        public List<GoodItem> GetRewardGoodItem(GuildRewardType guildRewardType, int step, int multiples = 1)
        {
            try
            {
                if (step >= _cache[guildRewardType].Count || step < 0) return null;
                
                var goodType = GetRewardGoodTypes(guildRewardType, step);
                var value = GetRewardValues(guildRewardType, step);
                var param0 = GetRewardParams(guildRewardType, step);

                return goodType.Select((t, i) => new GoodItem(t, value[i] * multiples, param0[i])).ToList();
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                return null;
            }
        }
        
        [CanBeNull]
        public GoodItem GetNeedGoodItem(GuildRewardType guildRewardType, int step)
        {
            try
            {
                step = Mathf.Min(step, _cache[guildRewardType].Count - 1);
                
                var goodType = GetNeedGoodTypes(guildRewardType, step);
                var value = GetNeedValue(guildRewardType, step);

                return new GoodItem(goodType, value);
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                return null;
            }
        }

        public int MaxPurchaseCount(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.maxPurchaseCount ?? 0;
        }
        
        public int CurrPurchaseCount(GuildRewardType type, int step)
        {
            var count = type switch
            {
                GuildRewardType.Shop => shopPurchaseCounts[step],
                _ => 0
            };
            return count;
        }

        public TimeResetType GetTimeResetType(GuildRewardType type, int step)
        {
            return GetCache(type, step)?.timeResetType ?? TimeResetType.None;
        }
        
        public void IncreaseCurrPurchaseCount(GuildRewardType type, int step, int count = 1)
        {
            switch (type)
            {
                case GuildRewardType.Shop:
                    shopPurchaseCounts[step] += count;
                    break;
            }
        }
        public void SetCurrPurchaseCount(GuildRewardType type, int step, int value)
        {
            switch (type)
            {
                case GuildRewardType.Shop:
                    shopPurchaseCounts[step] = value;
                    break;
            }
        }
    }
}