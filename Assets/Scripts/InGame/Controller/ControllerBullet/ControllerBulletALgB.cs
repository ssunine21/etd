using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALgB : ControllerBullet
    {
        private readonly ViewBulletALgB _view;
        
        public ControllerBulletALgB(CancellationTokenSource cts, Transform parent) 
            : base(cts, parent, InGame.View.View.Get<ViewBulletALgB>())
        {
            _view = (ViewBulletALgB)viewBullet;
            _view.LightningBolt.Init();       
            ParticleType = ParticleType.ParticleLg;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "ALgC"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
