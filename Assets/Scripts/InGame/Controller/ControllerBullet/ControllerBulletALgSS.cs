using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALgSS : ControllerBullet
    {
        private readonly ViewBulletALgSS _view;
        private bool _isAppear;
        
        public ControllerBulletALgSS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletALgSS>())
        {
            _view = (ViewBulletALgSS)viewBullet;
            ParticleType = ParticleType.ParticleLgBig;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "ALgA")) bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
