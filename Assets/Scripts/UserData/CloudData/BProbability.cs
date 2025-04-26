using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BProbability
    {
        public int index;
        public ProbabilityType probabilityType;
        public float[] probability;
    }
}