using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAWSS : ControllerBullet
    {
        private readonly ViewBulletAWSS _view;
        
        public ControllerBulletAWSS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAWSS>())
        {
            _view = (ViewBulletAWSS)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "AWA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
