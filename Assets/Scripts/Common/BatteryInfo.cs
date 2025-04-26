using System;
using UnityEngine;

namespace ETD.Scripts.Common
{
    public class BatteryInfo : MonoBehaviour
    {
        public float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }
    }
}