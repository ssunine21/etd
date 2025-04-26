using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALB : ControllerBullet
    {
        private readonly ViewBulletALB _view;

        public ControllerBulletALB(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, InGame.View.View.Get<ViewBulletALB>())
        {
            _view = (ViewBulletALB)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bulletA, "ALC"))
                bulletA.Shot(unit, enemy, nonTargets).Forget();
            
            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Add(enemy);
            
            if(EnemyManager.Instance.TryGetNearbyDamageable(unit.Position, out var eachEnemy, nonTargets))
            {
                if (TryCopyBullet(out var bulletB, "ALC"))
                    bulletB.Shot(unit, eachEnemy, nonTargets).Forget();
            }
        }
    }
}
