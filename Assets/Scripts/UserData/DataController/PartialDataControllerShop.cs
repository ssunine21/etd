using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataShop shop;
    }

    [Serializable]
    public class DataShop
    {
        public UnityAction<ProductType> onBindPurchased;
        public UnityAction OnBindVipPurchased;
        public UnityAction onBindInitialized;
        public UnityAction<string> onBindNewPackageTime;

        public List<int> currPurchasedCounts;
        public List<string> newPackageDisableTimeToString;

        [FormerlySerializedAs("isFirstPurchase")] public bool isFirstPurchased;
        public bool hasFirstPurchaseReward;
        
        private BShop[] BShops => CloudData.CloudData.Instance.bShops;
        private BInfo BInfo => CloudData.CloudData.Instance.bInfos[0];
        private Dictionary<ProductType, BShop> _cache;
        
        public void Init()
        {
            Caching();
            ServerTime.onBindNextDay += OnNextDay;
            ServerTime.onBindNextWeek += OnNextWeek;

            StageManager.Instance.onBindStageClear += (stageType) =>
            {
                if (stageType == StageType.Normal)
                    TryUnlockNewPackage(UnlockType.StageLevel, DataController.Instance.stage.currTotalLevel);
            };
        }

        public int GetProtectionDurationTimeInHour()
        {
            return BInfo.protectionDurationInHour;
        }
        
        public void TryUnlockNewPackage(UnlockType unlockType, int unlockValue)
        {
            const ProductType defaultProductType = ProductType.SummonRuneForDia;
            var controllerNewPackage = ControllerCanvas.Get<ControllerCanvasNewPackage>();
            var productType = defaultProductType;
            
            for (var i = 0; i < BShops.Length; ++i)
            {
                if(unlockType != BShops[i].unlockType) continue;
                if(!IsUnlock(unlockType, BShops[i].unlockValue, unlockValue)) continue;
                if(!string.IsNullOrEmpty(newPackageDisableTimeToString[i])) continue;
                
                newPackageDisableTimeToString[i] = ServerTime.DateTimeToIsoString(ServerTime.Date.AddHours(BShops[i].remainTime));
                controllerNewPackage.Add(BShops[i].productType);
                
                if (productType == defaultProductType)
                    productType = BShops[i].productType;
            }

            if (productType != defaultProductType)
                controllerNewPackage.ShowView(productType);
        }

        private bool IsRemainingTime(ProductType type)
        {
            var index = GetCache(type).index;
            var dateTimeToString = newPackageDisableTimeToString[index];
            
            if (string.IsNullOrEmpty(dateTimeToString)) return false;
            return ServerTime.IsRemainingTimeUntilDisable(dateTimeToString);
        }

        private bool IsUnlock(UnlockType unlockType, int unlockValue, int dynamicValue)
        {
            switch (unlockType)
            {
                case UnlockType.StageLevel:
                case UnlockType.GoldDungeon:
                case UnlockType.DiaDungeon:
                case UnlockType.EnhanceDungeon:
                    return unlockValue <= dynamicValue;
                case UnlockType.GetElemental:
                    return unlockValue == dynamicValue; 
            }

            return false;
        }

        public bool CanReceiveFirstPurchaseReward()
        {
            return !hasFirstPurchaseReward;
        }
        
        public void SetHasFirstPurchaseReward(bool flag)
        {
            hasFirstPurchaseReward = flag;
            DataController.Instance.LocalSave();
        }

        public bool IsFirstPurchased()
        {
            return isFirstPurchased;
        }
        
        public void SetIsFirstPurchased(bool flag)
        {
            isFirstPurchased = flag;
            DataController.Instance.LocalSave();
        }

        public GoodType GetGoodType(ProductType type)
        {
            return GetCache(type).needGoodType;
        }

        public int GetMaxPurchaseCount(ProductType type)
        {
            return GetCache(type).maxPurchaseCount;
        }

        public bool TryGetAlternativeGoods(ProductType type, out KeyValuePair<GoodType, double> keyValuePair)
        {
            keyValuePair = type switch
            {
                ProductType.SummonElementalForDia => new KeyValuePair<GoodType, double>(GoodType.SummonElementalTicket, 1),
                ProductType.SummonElementalForDias => new KeyValuePair<GoodType, double>(GoodType.SummonElementalTicket, 10),
                ProductType.SummonRuneForDia => new KeyValuePair<GoodType, double>(GoodType.SummonRuneTicket, 1),
                ProductType.SummonRuneForDias => new KeyValuePair<GoodType, double>(GoodType.SummonRuneTicket, 10),
                _ => new KeyValuePair<GoodType, double>(GoodType.None, 0)
            };
            
            return keyValuePair.Key != GoodType.None;
        }

        public double GetPrice(ProductType type)
        {
            return GetCache(type).needPrice;
        }

        public int GetPackageValue(ProductType type)
        {
            return GetCache(type).packageValue;
        }

        public TimeResetType GetTimeResetType(ProductType type)
        {
            return GetCache(type).timeResetType;
        }

        public List<ProductType> GetEnableNewPackageProductTypes()
        {
            var productTypes = new List<ProductType>();

            foreach (var bShop in BShops)
            {
                if (IsRemainingTime(bShop.productType)
                    && GetCurrPurchasedCount(bShop.productType) > 0)
                    productTypes.Add(bShop.productType);
            }
            return productTypes;
        }

        public string GetNewPackageDisableTimeToString(ProductType type)
        {
            var index = GetCache(type).index;
            return newPackageDisableTimeToString[index];
        }

        public int GetCurrPurchasedCount(ProductType type)
        {
            return currPurchasedCounts[GetCache(type).index];
        }

        public string GetProductId(ProductType type)
        {
            return GetCache(type).productId;
        }

        public void DiscountCurrPurchased(ProductType type, int count = 1)
        {
            if (GetMaxPurchaseCount(type) < 0) return;
            currPurchasedCounts[GetCache(type).index] =
                Mathf.Clamp(currPurchasedCounts[GetCache(type).index] - count, 0, GetMaxPurchaseCount(type));
        }

        public List<GoodItem> GetRewardGoodItems(ProductType type)
        {
            var goodTypes = GetRewardGoodTypes(type);
            var goodValues = GetRewardValues(type);
            var goodParams = GetRewardParam0(type);
            var goodItems = goodTypes.Select((goodType, i) => new GoodItem(goodType, goodValues[i], goodParams[i])).ToList();
            
            return goodItems;
        }

        public GoodType[] GetRewardGoodTypes(ProductType type)
        {
            return GetCache(type).rewardGoodTypes;
        }

        public double[] GetRewardValues(ProductType type)
        {
            return GetCache(type).rewardValues;
        }

        public int[] GetRewardParam0(ProductType type)
        {
            return GetCache(type).params0;
        }

        public KeyValuePair<GoodType, double> GetNeededGoods(ProductType type)
        {
            var cache = GetCache(type);
            return new KeyValuePair<GoodType, double>(cache.needGoodType, cache.needPrice);
        }

        public BShop GetCache(ProductType type)
        {
            return _cache[type];
        }

        private void OnNextDay()
        {
            for (var i = 0; i < BShops.Length; ++i)
            {
                if(currPurchasedCounts.Count <= i)
                    currPurchasedCounts.Add(BShops[i].maxPurchaseCount);

                if (BShops[i].timeResetType == TimeResetType.Daily)
                    currPurchasedCounts[i] = BShops[i].maxPurchaseCount;
            }
            onBindInitialized?.Invoke();
        }

        private void OnNextWeek()
        {
            for (var i = 0; i < BShops.Length; ++i)
            {
                if(currPurchasedCounts.Count <= i)
                    currPurchasedCounts.Add(BShops[i].maxPurchaseCount);

                if (BShops[i].timeResetType == TimeResetType.Weekly)
                    currPurchasedCounts[i] = BShops[i].maxPurchaseCount;
            }
            onBindInitialized?.Invoke();
        }

        private void Caching()
        {
            _cache ??= new Dictionary<ProductType, BShop>();
            currPurchasedCounts ??= new List<int>();
            newPackageDisableTimeToString ??= new List<string>();

            for (var i = 0; i < BShops.Length; ++i)
            {
                if(currPurchasedCounts.Count <= i)
                    currPurchasedCounts.Add(BShops[i].maxPurchaseCount);
                
                if(newPackageDisableTimeToString.Count <= i)
                    newPackageDisableTimeToString.Add(string.Empty);

                var productType = BShops[i].productType;
                _cache.TryAdd(productType, BShops[i]);
            }
        }
    }
}