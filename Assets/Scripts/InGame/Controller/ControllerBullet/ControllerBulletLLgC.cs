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
    public class ControllerBulletLLgC : ControllerBullet
    {
        private readonly ViewBulletLLgC _view;
        
        public ControllerBulletLLgC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLLgC>())
        {
            ChainCount = 0;
            _view = (ViewBulletLLgC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            nonTargets ??= new HashSet<IDamageable>();
            var basePosition = controllerBullet.Position;
                if (TryGetCollidedEnemy(out var nearbyEnemy, basePosition, 2.5f, nonTargets))
                {
                    nonTargets.Add(nearbyEnemy);
                    SpawnProjectile(out var bullet);
                    bullet.Shot(basePosition, nearbyEnemy).Forget();
                }
            nonTargets.Clear();
        }

        private void SpawnProjectile(out ControllerBullet bullet)
        {
            bullet = ObjectPoolManager.Instance.GetBullet("ALgC", ProjectorIndex);
            var isCritical = Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

            bullet.EquippedPositionType = EquippedPositionType;
            bullet.IsLinkable = false;
            bullet
                .SetPower(AttackPower, isCritical)
                .SetAbility(Size, Duration, ChainCount);
        }
    }
}
