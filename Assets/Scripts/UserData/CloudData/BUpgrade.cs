using System;
using ETD.Scripts.Common;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BUpgrade
    {
        public int index;
        public UpgradeType upgradeType;
        public LocalizedTextType localizedTextType;
        public int maxLevel;
        public float baseValue;
        public float incrementValue;
        public double coefficient;
        public GoodType goodType;
        public float baseCostValue;
        public float incrementCostValue;
        public double costCoefficient;
        public int order;
        public string expression;
    }
}