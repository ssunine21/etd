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
    public class ControllerBulletAWA : ControllerBullet
    {
        private readonly ViewBulletAWA _view;
        
        public ControllerBulletAWA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAWA>())
        {
            _view = (ViewBulletAWA)viewBullet;
            ParticleType = ParticleType.ParticleI;
            RotateSpeed = 105f;    
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            Shot(unit.Position, enemy.Position, nonTargets).Forget();
        }

        public override async UniTaskVoid Shot(Vector2 from, Vector2 to, HashSet<IDamageable> nonTargets = null)
        {
            var originMoveSpeed = MoveSpeed;
            MoveSpeed = 5f;
            
            _view.TrailRenderer.Clear();
            _view.SetActive(true);
            _view.Model.SetActive(false);
            
            Position = from;

            nonTargets ??= new HashSet<IDamageable>();
            
            while (_view.isActiveAndEnabled && !IsReachedTarget(to) && !Cts.IsCancellationRequested)
            {
                MoveToTarget(to);
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }

            MoveSpeed = originMoveSpeed;
            _view.Model.SetActive(true);
            AutoDisable().Forget();
            AutoDotAttack().Forget();
            //AutoLinkedBulletAttack().Forget();
            
            Direction = (to - Position).normalized;
            var targetPosition = Vector2.zero;

            while (_view.isActiveAndEnabled && !Cts.IsCancellationRequested)
            {
                targetPosition = EnemyManager.Instance.TryGetNearbyDamageable(Position, out var nextEnemy) ? nextEnemy.Position : targetPosition;
                MoveAroundToTarget(targetPosition);
                Rotate();
                
                if (IsDotAttackable())
                {
                    if (TryGetCollidedEnemies(out var targets))
                    {
                        foreach (var target in targets)
                        {
                            Attack(target);
                            ShowParticle(target.Position);
                        }
                    }
                    
                    if (ChainCount > 0)
                    {
                        if (TryCopyBullet(out var copyBullet))
                        {
                            copyBullet.ChainCount -= 1;
                            ChainCount = 0;

                            if (EnemyManager.Instance.TryGetNearbyDamageable(Vector2.zero, out var enemy, nonTargets))
                                copyBullet.Shot(Position, enemy.Position, nonTargets).Forget();
                            else
                                copyBullet.Shot(Position, Utility.RandomPositionInView()).Forget();
                        }

                        ChainCount = 0;
                    }
                }
                
                // if (IsLinkedBulletAttackable())
                // {
                //     if (TryCreateLinkBullet(out var linkBullet))
                //         linkBullet.Shot(this, null).Forget();
                // }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            
            _view.SetActive(false);
        }
    }
}
