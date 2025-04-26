using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BContentUnlock
    {
        public int index;
        public UnlockType unlockType;
        public int questIndex;
        public bool isShowAnimation;
    }
}