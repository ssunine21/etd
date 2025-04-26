using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletALS : ViewBullet
    {
#if IS_TEST
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            // 원의 중심 좌표로 기즈모를 이동
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector2.zero, radius);
        }
#endif
    }
}
