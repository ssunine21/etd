using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletLightRocket : ViewBullet
    {
        public TrailRenderer TrailRenderer => trailRenderer;
        [SerializeField] private TrailRenderer trailRenderer;

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