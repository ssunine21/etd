using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletADC : ControllerBullet
    {
        private readonly ViewBulletADC _view;
        
        private bool _isAppear;
        
        public ControllerBulletADC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletADC>())
        {
            _view = (ViewBulletADC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            Shot(enemy.Position, enemy.Position, nonTargets).Forget();
        }

        public override async UniTaskVoid Shot(Vector2 from, Vector2 to, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            _view.Model.SetActive(false);

            Position = from;
            nonTargets ??= new HashSet<IDamageable>();
            
            do
            {
                MoveToTarget(to);
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            } while (_view.isActiveAndEnabled && !IsReachedTarget(to) && !Cts.IsCancellationRequested);
            
            _view.Model.SetActive(true);
            _view.BlackHoleParticle.Play();

            AutoDisable().Forget();
            AutoDotAttack().Forget();
            //AutoLinkedBulletAttack().Forget();
            
            while (_view.isActiveAndEnabled && !Cts.IsCancellationRequested)
            {
                Rotate();
                
                if (IsDotAttackable())
                {
                    if (TryGetCollidedEnemies(out var targets))
                    {
                        foreach (var target in targets)
                        {
                            Attack(target);
                            PullEnemy(target);
                        }
                    }
                }

                if (ChainCount > 0)
                {
                    if (TryCopyBullet(out var copyBullet))
                    {
                        copyBullet.ChainCount -= 1;
                        if (EnemyManager.Instance.TryGetNearbyDamageable(Vector2.zero, out var enemy, nonTargets))
                        {
                            copyBullet.Shot(Position, enemy.Position, nonTargets).Forget();
                        }
                        else
                        {
                            copyBullet.Shot(Position, Utility.RandomPositionInView(), nonTargets).Forget();
                        }
                    }

                    ChainCount = 0;
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
        
        public override ControllerBullet SetAbility(float size = 0, float duration = 0, int chainCount = 0)
        {
            base.SetAbility(size, duration, chainCount);

            var particleMain = _view.BlackHoleParticle.main;
            var particleRays = _view.BlackHoleRaysParticle.main;
            var particlePulse = _view.BlackHolePulseParticle.main;
            
            particleMain.startSize = size * 0.8f;
            particleRays.startSize = size * 0.15f;
            particlePulse.startSize = size * 1.2f;
            
            return this;
        }
    }
}
