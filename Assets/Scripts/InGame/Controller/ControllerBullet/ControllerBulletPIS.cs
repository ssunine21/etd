using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletPIS : ControllerBullet
    {
        private readonly ViewBulletPIC _view;
        
        public ControllerBulletPIS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPIC>())
        {
            _view = (ViewBulletPIC)viewBullet; 
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "PIC"))
                bullet.Shot(position, nonTargets).Forget();
        }
    }
}
