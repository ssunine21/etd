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
    public class ControllerBulletAWC : ControllerBullet
    {
        private readonly ViewBulletAWC _view;
        
        public ControllerBulletAWC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletAWC>())
        {
            _view = (ViewBulletAWC)viewBullet;
            ParticleType = ParticleType.ParticleI;
            RotateSpeed = 55f;
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            Shot(unit.Position, enemy.Position, nonTargets).Forget();
        }

        public override async UniTaskVoid Shot(Vector2 from, Vector2 to, HashSet<IDamageable> nonTargets = null)
        {
            MoveSpeed = 5f;
            
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
                            ShowParticle(target.Position);
                        }
                    }
                    
                    if (ChainCount > 0)
                    {
                        if (TryCopyBullet(out var copyBullet))
                        {
                            copyBullet.ChainCount -= 1;
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
