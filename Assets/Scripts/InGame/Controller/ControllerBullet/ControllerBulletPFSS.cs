using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletPFSS : ControllerBullet
    {
        private readonly ViewBulletPFC _view;

        public ControllerBulletPFSS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPFC>())
        {
            _view = (ViewBulletPFC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet,  "PFC"))
                bullet.Shot(position, nonTargets).Forget();
        }
    }
}
