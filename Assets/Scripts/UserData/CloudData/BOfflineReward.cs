using System;
using ETD.Scripts.Common;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BOfflineReward
    {
        public int index;
        public GoodType goodType;
        public float valuePerMinute;
        public int totalTimeHour;
    }
}