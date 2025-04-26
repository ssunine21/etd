using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataBuff buff;
    }

    [Serializable]
    public class DataBuff
    {
        public List<int> currExps;
        public List<int> levels;
        public List<bool> usedFreeOnceBuffs;
        public List<int> remainTimeForSec;
        public List<bool> isBuffOn;

        public UnityAction OnBindUpdateBuff;
        public UnityAction OnBindGameSpeed;

        private BBuff[] BData => CloudData.CloudData.Instance.bBuffs;

        public void Init()
        {
            currExps ??= new List<int>();
            levels ??= new List<int>();
            usedFreeOnceBuffs ??= new List<bool>();
            remainTimeForSec ??= new List<int>();
            isBuffOn ??= new List<bool>();

            for (var i = currExps.Count; i < BData.Length; ++i) currExps.Add(0);
            for (var i = levels.Count; i < BData.Length; ++i) levels.Add(0);
            for (var i = usedFreeOnceBuffs.Count; i < BData.Length; ++i) usedFreeOnceBuffs.Add(false);
            for (var i = remainTimeForSec.Count; i < BData.Length; ++i) remainTimeForSec.Add(0);
            for (var i = isBuffOn.Count; i < BData.Length; ++i) isBuffOn.Add(false);
        }

        public bool IsBuffOn(int index)
        {
            return isBuffOn[index];
        }

        public void SetBuffOn(int index, bool flag)
        {
            isBuffOn[index] = flag;
        }

        public bool IsFree(int index)
        {
            return !usedFreeOnceBuffs[index];
        }

        public void UseFree(int index)
        {
            usedFreeOnceBuffs[index] = true;
            DataController.Instance.LocalSave();
        }

        public bool IsRemainTime(int index)
        {
            return remainTimeForSec[index] > 0;
        }

        public int GetRemainTime(int index)
        {
            return remainTimeForSec[index];
        }

        public void SetRemainTime(int index, int sec)
        {
            remainTimeForSec[index] = sec;
            DataController.Instance.LocalSave();
        }

        public void DecreaseRemainTime(int index, int amount = 1)
        {
            remainTimeForSec[index] -= amount;
            DataController.Instance.LocalSave();
        }

        public AttributeType GetAttributeType(int index)
        {
            return BData[index].attributeType;
        }

        public int GetIncreaseExp(int index)
        {
            return BData[index].increaseCurrExp;
        }

        public int GetLevel(int index)
        {
            return levels[index];
        }

        public void SetLevel(int index, int value)
        {
            levels[index] = value;
            DataController.Instance.LocalSave();
        }

        public int GetCurrExp(int index)
        {
            return currExps[index];
        }

        public void SetCurrExp(int index, int value)
        {
            currExps[index] = value;
            DataController.Instance.LocalSave();
        }

        public int GetMaxExp(int index)
        {
            if (BData.Length <= index) return 0;
            var maxExp = BData[index].baseMaxExp + BData[index].increaseMaxExp * levels[index];
            return maxExp;
        }

        public float GetValueIfBuffOn(AttributeType attributeType)
        {
            foreach (var bBuff in BData)
            {
                if (bBuff.attributeType == attributeType)
                {
                    if (IsBuffOn(bBuff.index))
                        return GetValue(bBuff.index);
                    return 0;
                }
            }

            return 0;
        }

        public float GetValue(int index)
        {
            if (BData.Length <= index) return 0;
            var value = BData[index].baseValue + BData[index].increaseValue * levels[index];
            return value;
        }
    }
}