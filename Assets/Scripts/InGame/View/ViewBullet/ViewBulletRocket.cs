using UnityEngine;
using UnityEngine.Serialization;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletRocket : ViewBullet
    {
        public TrailRenderer TrailRenderer => trailRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
    }
}
