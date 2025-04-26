using DigitalRuby.LightningBolt;
using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletALB : ViewBullet
    {
        public LightningBoltScript Laser => _laser;
        public ParticleSystem EndEndParticle => _endParticle;
        
        [SerializeField] private LightningBoltScript _laser;
        [SerializeField] private ParticleSystem _endParticle;
    }
}
