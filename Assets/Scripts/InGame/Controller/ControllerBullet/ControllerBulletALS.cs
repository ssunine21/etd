using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALS : ControllerBullet
    {
        private readonly ViewBulletALS _view;
        
        public ControllerBulletALS(CancellationTokenSource cts, Transform parent) 
            : base(cts, parent, View.View.Get<ViewBulletALS>())
        {
            _view = (ViewBulletALS)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "ALA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
