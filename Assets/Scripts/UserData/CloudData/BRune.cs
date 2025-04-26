using System;
using ETD.Scripts.Common;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BRune
    {
        public int index;
        public RuneType runeType;
        public GradeType grade;
        public AttributeType[] obtainableAttrTypes;
        public float[] obtainableAttrValues;
        public float[] dynamicAttrValueRange;
        public float[] enhanceEquipAttrIncreaseValues;
    }
}