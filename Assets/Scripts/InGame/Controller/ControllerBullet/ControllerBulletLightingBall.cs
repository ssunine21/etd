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
    public class ControllerBulletLightingBall : ControllerBullet
    {
        private readonly ViewBulletLightingBall _view;
        
        public ControllerBulletLightingBall(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLightingBall>())
        {
            _view = (ViewBulletLightingBall)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }

        public override async UniTaskVoid Shot(Vector2 basePosition, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            Position = basePosition;
            _view.UpdatePosition(Position);

            nonTargets ??= new HashSet<IDamageable>();

            var targetResetTime = 0f;
            while (_view.isActiveAndEnabled)
            {
                Position = _view.transform.parent.transform.TransformPoint(_view.transform.localPosition);
                if (TryGetCollidedEnemies(out var targets, nonTargets))
                {
                    foreach (var target in targets)
                    {
                        nonTargets.Add(target);
                    }
                    
                    foreach (var target in targets)
                    {
                        Attack(target);
                        ShowCollisionParticle(target.Position);

                        if (EnemyManager.Instance.TryGetNearbyDamageable(target.Position, out var nextEnemy, nonTargets))
                        {
                            if (ObjectPoolManager.Instance.TryGetBullet("ALgC", ProjectorIndex, out var bullet))
                            {
                                var isCritical =
                                    Utility.IsProbabilityTrue(
                                        DataController.Instance.player.GetCriticalRate(ProjectorIndex));
                                bullet
                                    .SetPower(AttackPower, isCritical)
                                    .SetAbility(0.15f, 0.25f, ChainCount)
                                    .Shot(Position, nextEnemy).Forget();
                            }
                        }
                    }
                }

                if (targetResetTime > 1)
                {
                    targetResetTime = 0f;
                    nonTargets.Clear();
                }

                targetResetTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            
            _view.SetActive(false);
        }
    }
}
