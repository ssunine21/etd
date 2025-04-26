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
    public class ControllerBulletAFS : ControllerBullet
    {
        private readonly ViewBulletAFS _view;
        
        public ControllerBulletAFS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAFS>())
        {
            _view = (ViewBulletAFS)viewBullet;
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
             if (TryCopyBullet(out var bulletA, "AFA"))
                 bulletA.Shot(unit, enemy, nonTargets).Forget();
        }
    }
}
