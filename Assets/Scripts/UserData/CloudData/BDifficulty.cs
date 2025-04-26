using System;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BDifficulty
    {
        public int index;
        public int level;
        public float basePowerValue;
        public float incrementPowerValue;
        public float baseHpValue;
        public float incrementHpValue;
        public float bossIncreaseValue;
    }
}