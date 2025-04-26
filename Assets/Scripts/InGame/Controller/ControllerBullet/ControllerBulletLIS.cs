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
    public class ControllerBulletLIS : ControllerBullet
    {
        private readonly ViewBulletLIS _view;
        
        public ControllerBulletLIS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLIS>())
        {
            _view = (ViewBulletLIS)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "LIC"))
                bullet.Shot(controllerBullet, enemy, nonTargets).Forget();
        }
    }
}
