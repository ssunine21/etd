using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BEnhancement
    {
        public int index;
        public EnhancementType enhancementType;
        public GradeType gradeType;
        public GoodType goodType;
        public double baseGoodValue;
        public double increaseGoodValue;
        public float[] probability;
        public int maxLevel;
        public double disassemblyMaterialValue;
    }
}