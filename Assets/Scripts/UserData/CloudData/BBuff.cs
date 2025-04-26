using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{   
    [Serializable]
    public class BBuff
    {
        public int index;
        public AttributeType attributeType;
        public float baseValue;
        public float increaseValue;
        public int baseMaxExp;
        public int increaseMaxExp;
        public int increaseCurrExp;
        public int maxLevel;
    }
}