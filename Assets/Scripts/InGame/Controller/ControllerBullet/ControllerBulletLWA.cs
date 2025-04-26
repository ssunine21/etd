using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;
// ReSharper disable All

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletLWA : ControllerBullet
    {
        private readonly ViewBulletLWA _view;
        
        public ControllerBulletLWA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLWA>())
        {
            _view = (ViewBulletLWA)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "LWC"))
                bullet.Shot(controllerBullet, enemy, nonTargets).Forget();
        }
    }
}
