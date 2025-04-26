using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BGood
    {
        public int index;
        public GoodType goodType;
        public LocalizedTextType titleLocalizeType;
        public LocalizedTextType descLocalizeType;
        public LocalizedTextType[] sourceLocalizeTypes;
        public LocalizedTextType[] usageLocalizeTypes;
    }
}