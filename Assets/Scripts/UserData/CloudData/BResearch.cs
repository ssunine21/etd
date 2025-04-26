using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BResearch
    {
        public int index;
        public ResearchType researchType;
        public LocalizedTextType localizeType;
        public float baseValue;
        public float increasementValue;
        public float baseCost;
        public float increasementCost;
        public float multiplierCost;
        public int baseTime;
        public int increasementTime;
        public float multiplierTime;
        public int maxLevel;
        public int slotIndex;
        public string expression;
        public float diaPerSecond;
    }
}