using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAWS : ControllerBullet
    {
        private readonly ViewBulletAWS _view;
        
        public ControllerBulletAWS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAWS>())
        {
            _view = (ViewBulletAWS)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "AWA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
