using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BElemental
    {
        public int index;
        public ElementalType elementalType;
        public GradeType grade;
        public AttributeType[] equippingEffAttrTypes;
        public float[] equippingEffAttrValues;
        public AttributeType[] possessionEffAttrTypes;
        public float[] possessionEffAttrValues;
        public float[] enhanceEquipAttrIncreaseValues;
        public float[] enhancePossessionAttrIncreaseValues;
        public float[] powerOfLevelAttrValues;
        public int[] attrLevels;
        public AttributeType[] levelAttrTypes;
        public float[] levelAttrValue;
    }
}