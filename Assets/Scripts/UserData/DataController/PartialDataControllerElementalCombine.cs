using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataElementalCombine elementalCombine;
    }

    [Serializable]
    public class DataElementalCombine
    {
        private Dictionary<string, BElementalCombine> _cache;
        
        public float GetSize(string key)
        {
            var data = GetElementalCombine(key);
            return data?.size ?? 0;
        }
        
        public float GetDuration(string key)
        {
            var data = GetElementalCombine(key);
            return data?.duration ?? 0f;
        }

        public float GetAttackSpeed(string key)
        {
            var data = GetElementalCombine(key);
            return data?.attackSpeed ?? 0f;
        }

        public float GetMoveSpeed(string key)
        {
            var data = GetElementalCombine(key);
            return data?.moveSpeed ?? 0;
        }

        public float GetAttackCoefficient(string key)
        {
            var data = GetElementalCombine(key);
            return data?.attackCoefficient ?? 1f;
        }

        public float GetAttackCountPerSecond(string key)
        {
            var data = GetElementalCombine(key);
            return data?.attackCountPerSecond ?? 0f;
        }

        public TagType[] GetTagTyeps(string key)
        {
            var data = GetElementalCombine(key);
            return data?.tags ?? Array.Empty<TagType>();
        }

        public BElementalCombine GetElementalCombine(string key)
        {
            if(string.IsNullOrEmpty(key)) return null;
            if(_cache == null) Init();
            
            return _cache.GetValueOrDefault(key, null);
        }

        private void Init()
        {
            _cache ??= new Dictionary<string, BElementalCombine>();
            foreach (var bElementalCombine in CloudData.CloudData.Instance.bElementalCombines)
            {
                _cache.TryAdd(bElementalCombine.key, bElementalCombine);
            }
        }
    }
}