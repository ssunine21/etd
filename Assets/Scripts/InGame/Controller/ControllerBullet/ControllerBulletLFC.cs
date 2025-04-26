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
    public class ControllerBulletLFC : ControllerBullet
    {
        private readonly ViewBulletLFC _view;
        
        public ControllerBulletLFC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLFC>())
        {
            _view = (ViewBulletLFC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy,
            HashSet<IDamageable> nonTargets = null)
        {
            var basePosition = enemy?.Position ?? controllerBullet.Position;
            SpawnProjectile(basePosition).Forget();
        }

        private async UniTaskVoid SpawnProjectile(Vector2 basePosition)
        {
            // 각 발사체의 회전 각도를 계산
            var angleStep = 13;
            var angle = Random.Range(0, 360);
                // 각 발사체의 방향을 계산
                var projectileDirXPosition = basePosition.x + Mathf.Sin((angle * Mathf.PI) / 180);
                var projectileDirYPosition = basePosition.y + Mathf.Cos((angle * Mathf.PI) / 180);

                var projectileVector = new Vector2(projectileDirXPosition, projectileDirYPosition);
             //   var projectileMoveDirection = (projectileVector - basePosition).normalized;

                var bullet = ObjectPoolManager.Instance.GetBullet("AFC", ProjectorIndex);
                var isCritical = Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

                bullet.IsLinkable = false;
                bullet.EquippedPositionType = EquippedPositionType;
                bullet
                    .SetPower(AttackPower, isCritical)
                    .SetAbility(Size, Duration, 0)
                    .Shot(basePosition, projectileVector).Forget();

                angle += angleStep;

        }
    }
}
