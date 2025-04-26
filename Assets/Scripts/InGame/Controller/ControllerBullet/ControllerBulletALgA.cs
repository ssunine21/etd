using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALgA : ControllerBullet
    {
        private readonly ViewBulletALgA _view;
        private bool _isAppear;
        
        public ControllerBulletALgA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, View.View.Get<ViewBulletALgA>())
        {
            _view = (ViewBulletALgA)viewBullet;
            _view.LightningBolt.Init();
            ParticleType = ParticleType.ParticleLgBig;
            RecoilDistance = 0.08f;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            Shot(unit.Position, enemy, nonTargets).Forget();
        }
        
         public override async UniTaskVoid Shot(Vector2 from, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            _view.LightningBolt.StartPosition = from;
            _view.LightningBolt.EndPosition = enemy.Position;
            _view.LightningBolt.Trigger();

            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Add(enemy);

            AutoDisable().Forget();
            AutoDotAttack().Forget();

            while (_view.isActiveAndEnabled && !Cts.IsCancellationRequested)
            {
                if (ChainCount > 0)
                {
                    if (EnemyManager.Instance.TryGetNearbyDamageable(Vector2.zero, out var nextEnemy, nonTargets))
                    {
                        if (TryCopyBullet(out var copyBullet))
                        {
                            copyBullet.ChainCount -= 1;
                            copyBullet.Shot(enemy.Position, nextEnemy, nonTargets).Forget();
                        }
                    }

                    ChainCount = 0;
                }
                else
                    nonTargets.Clear();
                
                _view.LightningBolt.StartPosition = from;
                _view.LightningBolt.EndPosition = enemy.Position;
                _view.LightningBolt.Trigger();

                if (enemy.IsActive && IsDotAttackable())
                {
                    if (TryGetCollidedEnemies(out var targets, enemy.Position, ColliderSize))
                    {
                        foreach (var target in targets)
                        {
                            ShowParticle(target.Position);
                            Attack(target);
                        }
                    }
                }

                // if (IsDotAttackable())
                // {
                //     if (enemy.IsActive)
                //     {                     
                //         ShowCollisionParticle(enemy.Position);
                //         Attack(enemy);
                //         
                //         if (TryCreateLinkBullet(out var linkBullet))
                //             linkBullet.Shot(this, enemy).Forget();
                //     }
                // }
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }
    }
}
