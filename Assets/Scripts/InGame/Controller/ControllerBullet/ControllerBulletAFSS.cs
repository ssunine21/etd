using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAFSS : ControllerBullet
    {
        private readonly ViewBulletAFSS _view;
        
        public ControllerBulletAFSS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAFSS>())
        {
            _view = (ViewBulletAFSS)viewBullet;
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "AFA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
