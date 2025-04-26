using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataContentUnlock contentUnlock;
    }

    [Serializable]
    public class DataContentUnlock
    {
        public List<bool> isOpenedList;
        public Dictionary<UnlockType, UnityAction> OnBindInitUnlockDic = new();
        private Dictionary<int, Dictionary<UnlockType, UnityAction<bool>>> _onBindUnlockDic = new();
        private BContentUnlock[] BDatas => CloudData.CloudData.Instance.bContentUnlocks;
        private Dictionary<UnlockType, int> _cache;
        
        public void Init()
        {
            _cache = new Dictionary<UnlockType, int>();
            OnBindInitUnlockDic = new Dictionary<UnlockType, UnityAction>();
            foreach (var data in BDatas)
            {
                _cache.TryAdd(data.unlockType, data.questIndex);
                AddListenerUnlock(data.unlockType, null);

                OnBindInitUnlockDic.TryAdd(data.unlockType, null);
            }

            isOpenedList ??= new List<bool>();
            for (var i = 0; i < BDatas.Length; ++i)
            {
                if(isOpenedList.Count == i)
                    isOpenedList.Add(!BDatas[i].isShowAnimation);
            }

            DataController.Instance.quest.OnBindClear += ContentsUnlock;

            UnlockTask().Forget();
        }

        public void SetOpen(UnlockType unlockType)
        {
            for (var i = 0; i < BDatas.Length; ++i)
            {
                if (BDatas[i].unlockType == unlockType)
                {
                    isOpenedList[i] = true;
                    DataController.Instance.LocalSave();
                    return;
                }
            }
        }

        public bool IsOpened(UnlockType unlockType)
        {
            for (var i = 0; i < BDatas.Length; ++i)
            {
                if (BDatas[i].unlockType == unlockType)
                    return isOpenedList[i];
            }

            return false;
        }
        
        public bool IsUnLock(UnlockType type)
        {
            var valueOrDefault = GetUnlockQuestLevel(type);
            return DataController.Instance.quest.currQuestLevel >= valueOrDefault;
        }
        
        public bool IsUnLock(int qusetIndex)
        {
            return DataController.Instance.quest.currQuestLevel >= qusetIndex;
        }

        public int GetUnlockQuestLevel(UnlockType type)
        {
            return _cache.GetValueOrDefault(type, 0);
        }

        public void AddListenerUnlock(UnlockType type, UnityAction<bool> callback)
        {
            var questLevel = GetUnlockQuestLevel(type);
            if (!_onBindUnlockDic.ContainsKey(questLevel))
            {
                _onBindUnlockDic.TryAdd(questLevel, new Dictionary<UnlockType, UnityAction<bool>>());
            }
            
            if (!_onBindUnlockDic[questLevel].ContainsKey(type))
            {
                _onBindUnlockDic[questLevel].TryAdd(type, null);
            }

            _onBindUnlockDic[questLevel][type] += callback;
        }
        
        public bool IsShowAnimation(int questLevel)
        {
            if (_onBindUnlockDic.TryGetValue(questLevel, out var dic))
                foreach (var onBindUnlock in dic)
                    if (IsShowAnimation(onBindUnlock.Key))
                        return true;
            return false;
        }

        public bool IsShowAnimation(UnlockType unlockType)
        {
            var data = BDatas.FirstOrDefault(x => x.unlockType == unlockType);
            return data?.isShowAnimation ?? false;
        }

        public Sprite GetUnlockedSprite(UnlockType type)
        {
            return ResourcesManager.Instance.GetUnlockImage(type);
        }

        private void ContentsUnlock(int questLevel)
        {
            if (_onBindUnlockDic.TryGetValue(questLevel, out var dic))
            {
                AsyncUnlock(dic);
                foreach (var onBindUnlock in dic)
                {
                    if (IsShowAnimation(onBindUnlock.Key))
                    {
                        ControllerCanvas.Get<ControllerCanvasRelease>().Show(onBindUnlock.Key);
                    }
                }
            }
        }
        
        private void AsyncUnlock(Dictionary<UnlockType,UnityAction<bool>> dic)
        {
            foreach (var onBindUnlock in dic)
            {
                onBindUnlock.Value?.Invoke(IsUnLock(onBindUnlock.Key));
            }
        }

        private async UniTaskVoid UnlockTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            
            foreach (var onBindUnlock in _onBindUnlockDic)
            {
                AsyncUnlock(onBindUnlock.Value);
            }
        }
    }
}