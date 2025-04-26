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
    public class ControllerBulletAIA : ControllerBullet
    {
        private readonly ViewBulletAIA _view;
        
        public ControllerBulletAIA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAIA>())
        {
            _view = (ViewBulletAIA)viewBullet;
            ParticleType = ParticleType.ParticleI;
            RecoilDistance = 0.08f;
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            Shot(unit.Position, enemy.Position, nonTargets).Forget();
        }

          public override async UniTaskVoid Shot(Vector2 from, Vector2 to, HashSet<IDamageable> nonTargets = null)
        {          
            _view.TrailRenderer.Clear();
            _view.SetActive(true);
            Position = from;
            LastPosition = from;
            Direction = (to - from).normalized;
            
            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Clear();
            
            AutoDisable().Forget();

            while (_view.isActiveAndEnabled && Direction != Vector2.zero && !Cts.IsCancellationRequested)
            {
                MoveToDicrect();
                //if (TryGetCollidedEnemies(out var targets, nonTargets))
                if (TryGetCollidedEnemies(LastPosition, Position, ColliderSize, out var targets, nonTargets))
                {
                    foreach (var target in targets)
                    {
                        Attack(target);
                        ShowParticle(target.Position);
                    
                        nonTargets.Add(target);
                        
                        if (ChainCount > 0)
                        {                 
                            if (EnemyManager.Instance.TryGetNearbyDamageable(target.Position, out var enemy, nonTargets))
                            {
                                if (TryCopyBullet(out var copyBullet))
                                {
                                    copyBullet.ChainCount -= 1;
                                    copyBullet.Shot(target.Position, enemy.Position).Forget();
                                }
                            }

                            ChainCount = 0;
                        }
                    }
                    
                    // if (!LinkBulletAttacked)
                    // {
                    //     if (TryCreateLinkBullet(out var linkBullet))
                    //     {
                    //         linkBullet.Shot(this, null).Forget();
                    //         LinkBulletAttacked = true;
                    //     }
                    // }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            _view.SetActive(false);
        }
          
        public override ControllerBullet SetAbility(float size = 0, float duration = 0, int chainCount = 0)
        {
            base.SetAbility(size, duration, chainCount);
            _view.TrailRenderer.startWidth = Size * 0.6f;
            return this;
        }
    }
}
