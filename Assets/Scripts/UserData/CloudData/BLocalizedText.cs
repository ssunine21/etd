using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BLocalizedText
    {
        public int index;
        public LocalizedTextType localizedTextType;
        public string[] countries;
    }
}