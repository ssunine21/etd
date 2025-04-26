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
    public class ControllerBulletDarkCircle : ControllerBullet
    {
        private readonly ViewBulletDarkCircle _view;
        
        public ControllerBulletDarkCircle(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletDarkCircle>())
        {
            _view = (ViewBulletDarkCircle)viewBullet;
            MoveSpeed = 3f;
            
            CollisionParticle = ObjectPoolManager.Instance.GetParticle(ParticleType.ParticleF);
        }

        public void SetActiveEmber(bool flag)
        {
            _view.GoEmber.SetActive(flag);
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 endPos, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            AutoDisable().Forget();
            AutoDotAttack().Forget();

            Position = endPos;
            _view.UpdatePosition(Position);
            _view.UpdateSize(0.02f);
            ScaleUp();

            while (_view.isActiveAndEnabled)
            {
                var collSize = _view.radius * _view.transform.localScale.x;
                if(IsDotAttackable())
                {
                    TryGetCollidedEnemies(out var innerEnemies, ColliderSize - 0.35f);
                    
                    if(innerEnemies != null)
                    {
                        nonTargets ??= new HashSet<IDamageable>();
                        foreach (var innerEnemy in innerEnemies)
                        {
                            nonTargets.Add(innerEnemy);
                        }
                    }

                    if (TryGetCollidedEnemies(out var targets, ColliderSize, nonTargets))
                    {
                        foreach (var target in targets)
                        {
                            Attack(target);
                            ShowCollisionParticle(target.Position);
                        }
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }
        
        private void ScaleUp()
        {
            _view.transform.DOScale(Vector3.one * Size, MoveSpeed);
        }
        
    }
}
