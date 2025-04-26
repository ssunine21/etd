using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletADB : ControllerBullet
    {
        private readonly ViewBulletADB _view;
        
        private bool _isAppear;
        
        public ControllerBulletADB(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletADB>())
        {
            _view = (ViewBulletADB)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var copyBullet, "ADC"))
                copyBullet.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
