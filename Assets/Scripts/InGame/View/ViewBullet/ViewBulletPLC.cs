using UnityEngine;
using UnityEngine.VFX;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletPLC : ViewBullet
    {
        public VisualEffect HealingEffect => healingEffect;
        
        [SerializeField] private VisualEffect healingEffect;
        
        #if IS_TEST
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            
            // 원의 중심 좌표로 기즈모를 이동
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
        #endif
    }
}
