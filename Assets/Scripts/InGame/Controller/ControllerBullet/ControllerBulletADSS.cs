using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletADSS : ControllerBullet
    {
        private readonly ViewBulletADSS _view;
        private bool _isAppear;
        
        public ControllerBulletADSS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletADSS>())
        {
            _view = (ViewBulletADSS)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var copyBullet, "ADC"))
                copyBullet.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
