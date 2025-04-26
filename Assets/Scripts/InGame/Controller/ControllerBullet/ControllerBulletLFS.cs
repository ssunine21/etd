using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletLFS : ControllerBullet
    {
        private readonly ViewBulletLFS _view;

        public ControllerBulletLFS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletLFS>())
        {
            _view = (ViewBulletLFS)viewBullet;
        }

        public override UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            return new UniTaskVoid();
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "LFA"))
                bullet.Shot(controllerBullet, enemy, nonTargets).Forget();
        }
    }
}