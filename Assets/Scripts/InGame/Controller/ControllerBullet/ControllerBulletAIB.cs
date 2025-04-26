using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAIB : ControllerBullet
    {
        private readonly ViewBulletAIB _view;
        
        public ControllerBulletAIB(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAIB>())
        {
            _view = (ViewBulletAIB)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "AIC"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
