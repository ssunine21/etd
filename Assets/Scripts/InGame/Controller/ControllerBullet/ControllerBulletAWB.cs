using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAWB : ControllerBullet
    {
        private readonly ViewBulletAWB _view;
        
        public ControllerBulletAWB(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAWB>())
        {
            _view = (ViewBulletAWB)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            Key = "AWC";
            if (TryCopyBullet(out var bulletA, "AWC"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
