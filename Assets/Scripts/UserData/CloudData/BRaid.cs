using System;
using ETD.Scripts.Common;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BRaid
    {
        public int index;
        public int maxLevel;
        public int[] placeCounts;
        public GoodType goodType;
        public double[] goodValuesPerSeconds;
        public int[] totalDurations;
        public int[] difficultyLevels;
    }
}