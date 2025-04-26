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
    public class ControllerBulletLWC : ControllerBullet
    {
        private readonly ViewBulletLWC _view;
        
        public ControllerBulletLWC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLWC>())
        {
            _view = (ViewBulletLWC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
                SpawnProjectile(controllerBullet.Position, 30 * 1);//attackcount
                await UniTask.Delay(100, false, PlayerLoopTiming.Update, Cts.Token);
        }

        private void SpawnProjectile(Vector2 basePosition, float baseAngle)
        {
            var bullet = ObjectPoolManager.Instance.GetBullet("TonadoW", ProjectorIndex);
            var isCritical =
                Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

            bullet.IsLinkable = false;
            bullet.EquippedPositionType = EquippedPositionType;
            bullet
                .SetPower(AttackPower, isCritical)
                .SetAbility(Size, Duration, 0);

            ((ControllerBulletTonadoW)bullet).Angle = baseAngle;

            bullet.AttackCountPerSecond = AttackCountPerSecond;
            basePosition += Random.insideUnitCircle;
            bullet.Shot(basePosition).Forget();
        }
    }
}
