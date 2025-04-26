using DigitalRuby.LightningBolt;
using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletALgB : ViewBullet
    {
        public LightningBoltScript LightningBolt => lightningBolt;
        [SerializeField] private LightningBoltScript lightningBolt;
    }
}
