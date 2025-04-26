using DigitalRuby.LightningBolt;
using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletAIS : ViewBullet
    {
        public LightningBoltScript Laser => _laser;
        [SerializeField] private LightningBoltScript _laser;
        
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
