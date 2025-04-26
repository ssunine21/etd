using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALSS : ControllerBullet
    {
        private readonly ViewBulletALSS _view;
        
        public ControllerBulletALSS(CancellationTokenSource cts, Transform parent) 
            : base(cts, parent, View.View.Get<ViewBulletALSS>())
        {
            _view = (ViewBulletALSS)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "ALA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
