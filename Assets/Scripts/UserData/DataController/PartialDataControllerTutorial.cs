using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataTutorial tutorial;
    }

    [Serializable]
    public class DataTutorial
    {
        private Dictionary<TutorialType, List<BTutorial>> _cache;
        private Dictionary<int, TutorialType> _cacheFromQuestLevel;

        public BTutorial Get(TutorialType type)
        {
            return _cache[type][0];
        }
        public int GetOrderCount(TutorialType type)
        {
            return _cache[type].Count;
        }
        
        public string GetDescription(TutorialType type, int order)
        {
            var localizedTextType = _cache[type][order].localizedTextType;
            return LocalizeManager.GetText(localizedTextType);
        }

        public int GetQuestLevel(TutorialType type)
        {
            return _cache[type][0].questLevel;
        }

        public LocalizedTextType? GetLocalizedTextType(TutorialType type, int order)
        {
            if(_cache.TryGetValue(type, out var value))
            {
                if (value.Count > order)
                    return value[order].localizedTextType;
            }

            return null;
        }

        public bool TryGetQuestType(int questLevel, out TutorialType value)
        {
            return _cacheFromQuestLevel.TryGetValue(questLevel, out value);
        }
        
        public void Init()
        {
            _cache ??= new Dictionary<TutorialType, List<BTutorial>>();     
            _cacheFromQuestLevel = new Dictionary<int, TutorialType>();

            foreach (var bTutorial in CloudData.CloudData.Instance.bTutorials)
            {
                if (!_cache.ContainsKey(bTutorial.tutorialType))
                    _cache.TryAdd(bTutorial.tutorialType, new List<BTutorial>());
                _cache[bTutorial.tutorialType].Add(bTutorial);

                _cacheFromQuestLevel.TryAdd(bTutorial.questLevel, bTutorial.tutorialType);
            }
        }
    }
}