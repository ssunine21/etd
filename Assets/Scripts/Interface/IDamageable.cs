using ETD.Scripts.InGame.View;
using UnityEngine;

namespace ETD.Scripts.Interface
{
    public interface IDamageable
    {
        public void Damaged(double damage, bool isCritical);
        public Vector2 Position { get; set; }
        public bool IsActive { get;}
        public View View { get; }
    }
}