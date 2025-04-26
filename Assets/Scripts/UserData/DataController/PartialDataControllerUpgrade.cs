using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataUpgrade upgrade;
    }

    [Serializable]
    public class DataUpgrade
    {
        public List<BUpgrade> CloudDataOrderBy =>
            _cloudData ??= new List<BUpgrade>(CloudData.CloudData.Instance.bUpgrades.OrderBy(x => x.order).ToList());

        public Dictionary<UpgradeType, UnityAction<int>> onBindUpgrade;
        public List<int> upgradeLevel;

        private List<BUpgrade> _cloudData;
        
        private Dictionary<UpgradeType, BUpgrade> _cache;
        private Dictionary<UpgradeType, double[]> _priceCache;
        private Dictionary<UpgradeType, double[]> _valueCache;
        
        public void Init()
        {
            Caching();
        }

        public bool IsMaxLevel(UpgradeType type)
        {
            var cloudData = GetCloudData(type);
            var currLevel = GetLevel(type);
            return cloudData.maxLevel <= currLevel;
        }

        public BUpgrade GetCloudData(UpgradeType type)
        {
            return _cache[type];
        }

        public Sprite GetImage(UpgradeType type)
        {
            return ResourcesManager.Instance.upgradeImages
                [Mathf.Clamp((int)type, 0, ResourcesManager.Instance.upgradeImages.Length - 1)];
        }

        public double GetUpgradePrice(UpgradeType type)
        {
            var level = Mathf.Clamp(GetLevel(type), 0, _priceCache[type].Length - 1);
            return _priceCache[type][level];
        }

        public int GetLevel(UpgradeType type)
        {
            return upgradeLevel[(int)type];
        }

        public double GetValue(UpgradeType type)
        {
            var level = GetLevel(type);
            if (level == 0) return 0;
            if (_valueCache.TryGetValue(type, out var value))
            {
                return value[Mathf.Clamp(level, 0, value.Length - 1)];
            }

            return 0;
            // var data = GetCloudData(type);
            // var value = data.baseValue + (data.incrementValue * GetLevel(type));
            // return value;
        }

        public string GetValueText(UpgradeType type)
        {
            var data = GetCloudData(type);
            return string.Format(data.expression, GetValue(type));
        }

        public void Upgrade(UpgradeType type, int increaseCount = 1)
        {
            upgradeLevel[(int)type] += increaseCount;
            DataController.Instance.mission.Count(MissionType.Upgrade, increaseCount);
            onBindUpgrade[type]?.Invoke(upgradeLevel[(int)type]);
        }

        public void InitUpgradeLevel()
        {
            for (var i = 0; i < upgradeLevel.Count; ++i)
            {
                upgradeLevel[i] = 0;
            }

            foreach (var upgradeDic in onBindUpgrade)
            {
                upgradeDic.Value?.Invoke(0);
            }
        }

        public bool IsLockElemental(int projectorIndex, int position)
        {
            return projectorIndex * 2 + position > GetValue(UpgradeType.IncreaseElementalUnit);
        }
        
        public bool IsLockRune(int projectorIndex, int position)
        {
            return projectorIndex * 3 + position >= GetValue(UpgradeType.IncreaseRuneUnit);
        }
        
        private void Caching()
        {
            _cache ??= new Dictionary<UpgradeType, BUpgrade>();
            upgradeLevel ??= new List<int>();
            onBindUpgrade ??= new Dictionary<UpgradeType, UnityAction<int>>();
            _priceCache ??= new Dictionary<UpgradeType, double[]>();
            _valueCache ??= new Dictionary<UpgradeType, double[]>();
            
            foreach (var bData in CloudDataOrderBy)
            {
                _cache.TryAdd(bData.upgradeType, bData);
            }

            foreach (var upgradeType in Enum.GetValues(typeof(UpgradeType)))
            {
                var type = (UpgradeType)upgradeType;
                onBindUpgrade.TryAdd(type, (index) => { });
                _priceCache.TryAdd(type, new double[_cache[type].maxLevel + 1]);
                _valueCache.TryAdd(type, new double[_cache[type].maxLevel + 1]);
            }

            foreach (var (upgradeType, data) in _cache)
            {
                for (var i = 0; i <= data.maxLevel; ++i)
                {
                    var cost =
                        (data.baseCostValue * Math.Pow(data.costCoefficient, i))
                        + (data.incrementCostValue * i);
                    _priceCache[upgradeType][i] = cost;

                    var value =
                        data.baseValue * Math.Pow(data.coefficient, i)
                        + data.incrementValue * (i);
                    _valueCache[upgradeType][i] = value;
                }
            }

            for (var i = upgradeLevel.Count; i < CloudData.CloudData.Instance.bUpgrades.Length; ++i)
            {
                upgradeLevel.Add(0);
            }
            
            
        }
    }
}