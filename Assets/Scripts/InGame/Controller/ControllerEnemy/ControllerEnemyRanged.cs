using System;
using System.Runtime.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller.ControllerBullet;
using ETD.Scripts.InGame.View.ViewEnemy;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ETD.Scripts.InGame.Controller.ControllerEnemy
{
    public class ControllerEnemyRanged : ControllerEnemy
    {
        private readonly ViewEnemyRanged _view;

        public ControllerEnemyRanged(CancellationTokenSource cts) : 
            base(cts, InGame.View.View.Get<ViewEnemyRanged>("Enemy"), EnemyType.Range)
        {
            _view = (ViewEnemyRanged)viewEnemy;
            _view.Damageable = this;
            MainTask().Forget();
        }

        public sealed override async UniTaskVoid MainTask()
        {
            var attackTime = 0f;

            while (IsActive)
            {
                TrackingToMainUnit();
                Rotate();
                UpdateView();
                
                if(!IsInView)
                    IsInView = Utility.IsInView(Position);

                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                if (IsAttackable)
                {
                    if (attackTime > 1 / AttackSpeed)
                    {
                        for (var i = 0; i < 3; ++i)
                        {
                            Attack(false);
                            await UniTask.Delay(30);
                        }
                        Attack(true);
                        attackTime = 0;
                    }

                    attackTime += Time.deltaTime;
                }
            }

            IsInView = false;
        }

        private void Attack(bool isAttack)
        {
            var mainUnit = MainUnitManager.Instance.MainUnitController;
            if(ObjectPoolManager.Instance.TryGetBullet("Rocket", 0, out var bullet))
            {
                var unitPos = Position + (Random.insideUnitCircle * _view.ColliderRange);

                bullet.SetPower(isAttack ? Power : 0, false);

                ((ControllerBulletRocket)bullet).Shot(unitPos, mainUnit.Position).Forget();
            }
        }
    }
}