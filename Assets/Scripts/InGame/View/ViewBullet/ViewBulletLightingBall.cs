using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletLightingBall : ViewBullet
    {
        public Transform ParticleTr => particleTr;
        [SerializeField] private Transform particleTr;
        
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
