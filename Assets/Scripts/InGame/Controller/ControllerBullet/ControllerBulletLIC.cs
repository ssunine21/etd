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
    public class ControllerBulletLIC : ControllerBullet
    {
        private readonly ViewBulletLIC _view;
        private const string SubKey = "DropWater";
        
        public ControllerBulletLIC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLIC>())
        {
            _view = (ViewBulletLIC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy,
            HashSet<IDamageable> nonTargets = null)
        {
                var angle = Random.Range(0f, Mathf.PI * 2);
                var distance = Random.Range(0f, controllerBullet.ColliderSize + 0.7f);

                // 각도와 거리를 이용하여 좌표 계산
                var x = controllerBullet.Position.x + distance * Mathf.Cos(angle);
                var y = controllerBullet.Position.y + distance * Mathf.Sin(angle);

                var randomPosition = new Vector2(x, y);
                SpawnProjectile(randomPosition);
                
                var delayTime = Random.Range(100, 300);
                await UniTask.Delay(delayTime, false, PlayerLoopTiming.Update, Cts.Token);
        }

        private void SpawnProjectile(Vector2 basePosition)
        {
            var bullet = ObjectPoolManager.Instance.GetBullet(SubKey, ProjectorIndex);
            var isCritical =
                Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

            bullet.IsLinkable = false;
            bullet.EquippedPositionType = EquippedPositionType;
            bullet
                .SetPower(AttackPower, isCritical)
                .SetAbility(Size, Duration, 0);

            bullet.AttackCountPerSecond = AttackCountPerSecond;
            bullet.Shot(basePosition).Forget();
        }
    }
}
