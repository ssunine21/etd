using ETD.Scripts.Common;
using UnityEngine;

namespace ETD.Scripts.Interface
{
    public interface IEnhanceable
    {
        public EnhancementType EnhancementType { get; }
        public GradeType GradeType { get; }
        public int EnhancementLevel { get; }
        public int EquippedIndex { get; }
        public Sprite IconSprite { get; } 

        public void Enhance();
    }
}