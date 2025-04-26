using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BAttribute
    {
        public int index;
        public AttributeType type;
        public string expression;
    }
}