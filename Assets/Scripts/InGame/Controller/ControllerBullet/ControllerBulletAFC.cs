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
    public class ControllerBulletAFC : ControllerBullet
    {
        private readonly ViewBulletAFC _view;
        
        public ControllerBulletAFC(CancellationTokenSource cts, Transform parent) : base(cts, parent,  View.View.Get<ViewBulletAFC>())
        {
            _view = (ViewBulletAFC)viewBullet;
            ParticleType = ParticleType.ParticleF;
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
            Direction = (to - from).normalized;
            
            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Clear();
            
            AutoDisable().Forget();

            while (_view.isActiveAndEnabled && Direction != Vector2.zero && !Cts.IsCancellationRequested)
            {
                MoveToDicrect();
                if (TryGetCollidedEnemy(out var target, nonTargets))
                {
                    if (TryCreateLinkBullet(out var linkBullet))
                        linkBullet.Shot(this, null).Forget();

                    Attack(target);
                    ShowParticle(target.Position);

                    if (ChainCount > 0)
                    {
                        nonTargets.Add(target);
                        if (EnemyManager.Instance.TryGetNearbyDamageable(target.Position, out var enemy, nonTargets)
                            && TryCopyBullet(out var copyObject))
                        {
                            copyObject.ChainCount -= 1;
                            copyObject.Shot(target.Position, enemy.Position).Forget();
                        }
                    }

                    break;
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
