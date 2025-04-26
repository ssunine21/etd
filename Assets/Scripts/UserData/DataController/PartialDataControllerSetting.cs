using System;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataSetting setting;
    }

    [Serializable]
    public class DataSetting
    {
        public UnityAction onBindInitPowerSaveTime;
        public UnityAction<bool> onBindBGM;
        public UnityAction<bool> onBindSFX;
        
        public string nextDayToString;
        public string nextWeekToString;
        
        public int gameSpeedremainTimeForSec;

        public bool isBGM = true;
        public bool isSFX = true;
        public bool isCameraShake = true;
        public bool isAutoSleep = true;
        public bool isGameSpeedUp;

        public bool dungeonNextChallenge;

        public bool is30fps;
        public bool is60fps = true;

        public float GameSpeed => _gameSpeed;
        private const float _gameSpeed = 0.3f;

        public bool isSummonEffectSkip;

        public string updateAt;
        
        public bool isGuideElemental;

        public void Init()
        {
            dungeonNextChallenge = false;
        }
    }
}