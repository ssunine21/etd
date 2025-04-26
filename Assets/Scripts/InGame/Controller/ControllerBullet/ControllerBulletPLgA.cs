using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletPLgA : ControllerBullet
    {
        private readonly ViewBulletPLgC _view;
        
        public ControllerBulletPLgA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPLgC>())
        {
            _view = (ViewBulletPLgC)viewBullet; 
            RotateSpeed = 25f;
            ChainCount = 0;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "PLgC"))
                bullet.Shot(position, nonTargets).Forget();
        }
    }
}