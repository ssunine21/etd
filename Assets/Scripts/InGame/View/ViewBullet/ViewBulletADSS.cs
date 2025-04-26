using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletADSS : ViewBullet
    {
        public ParticleSystem BlackHoleParticle => _blackParticle;
        public ParticleSystem BlackHoleRaysParticle => _blackHoleRaysParticle;
        public ParticleSystem BlackHolePulseParticle => _blackHolePulseParticle;
        [SerializeField] private ParticleSystem _blackParticle;
        [SerializeField] private ParticleSystem _blackHoleRaysParticle;
        [SerializeField] private ParticleSystem _blackHolePulseParticle;
        
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
