using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletADS : ControllerBullet
    {
        private readonly ViewBulletADS _view;
        
        private bool _isAppear;
        
        public ControllerBulletADS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletADS>())
        {
            _view = (ViewBulletADS)viewBullet;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var copyBullet, "ADC"))
                copyBullet.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
