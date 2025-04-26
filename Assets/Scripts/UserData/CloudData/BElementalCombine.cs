using System;
using ETD.Scripts.Common;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BElementalCombine
    {
        public int index;
        public string key;
        public TagType[] tags;
        public float size;
        public float attackSpeed;
        public float moveSpeed;
        public float duration;
        public float attackCountPerSecond;
        public float attackCoefficient;
        public LocalizedTextType descTextType;
    }
}