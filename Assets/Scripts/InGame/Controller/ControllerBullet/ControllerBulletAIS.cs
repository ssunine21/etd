using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAIS : ControllerBullet
    {
        private readonly ViewBulletAIS _view;
        
        public ControllerBulletAIS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAIS>())
        {
            _view = (ViewBulletAIS)viewBullet;
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "AIA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}