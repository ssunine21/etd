using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALgS : ControllerBullet
    {
        private readonly ViewBulletALgS _view;
        private bool _isAppear;

        public ControllerBulletALgS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletALgS>())
        {
            _view = (ViewBulletALgS)viewBullet;
            ParticleType = ParticleType.ParticleLgBig;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "ALgA")) bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
