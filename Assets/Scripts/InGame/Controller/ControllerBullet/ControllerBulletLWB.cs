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
    public class ControllerBulletLWB : ControllerBullet
    {
        private readonly ViewBulletLWB _view;
        
        public ControllerBulletLWB(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLWB>())
        {
            _view = (ViewBulletLWB)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "LWC"))
                bullet.Shot(controllerBullet, enemy, nonTargets).Forget();
        }
    }
}
