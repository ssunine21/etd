using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletAIC : ViewBullet
    {
        public TrailRenderer TrailRenderer => trailRenderer;
        [SerializeField] private TrailRenderer trailRenderer;

        #if IS_TEST
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
        #endif
    }
}
