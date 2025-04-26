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
    public class ControllerBulletPIC : ControllerBullet
    {
        private readonly ViewBulletPIC _view;
        
        public ControllerBulletPIC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPIC>())
        {
            _view = (ViewBulletPIC)viewBullet; 
            CollisionParticle = ObjectPoolManager.Instance.GetParticle(ParticleType.ParticleI);
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            AutoDotAttack().Forget();
            
            while (_view.isActiveAndEnabled)
            {
                if (IsDotAttackable())
                {
                    if (TryGetCollidedEnemies(out var targets, Vector2.zero, 50))
                    {
                        foreach (var target in targets)
                        {
                            IsCritical =  
                            Utility.IsProbabilityTrue(
                                DataController.Instance.player.GetCriticalRate(ProjectorIndex));

                            Attack(target);
                            ShowCollisionParticle(target.Position);
                        }
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }
    }
}
