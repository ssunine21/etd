using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletAISS : ControllerBullet
    {
        private readonly ViewBulletAISS _view;
        
        public ControllerBulletAISS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAISS>())
        {
            _view = (ViewBulletAISS)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "AIA"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
