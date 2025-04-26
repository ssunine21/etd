using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BAttendance
    {
        public int index;
        public AttributeType[] attendanceType;
        public GoodType[] goodTypes;
        public float[] values;
        public int[] params0;
    }
}