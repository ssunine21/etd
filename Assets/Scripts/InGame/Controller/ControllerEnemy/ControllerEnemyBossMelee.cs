using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewEnemy;
using ETD.Scripts.Manager;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerEnemy
{
    public class ControllerEnemyBossMelee : ControllerEnemy
    {
        private readonly ViewEnemyBossMelee _view;
        private ParticleSystem _attackParticle;
        
        public ControllerEnemyBossMelee(CancellationTokenSource cts) 
            : base(cts, InGame.View.View.Get<ViewEnemyBossMelee>("Enemy"), EnemyType.BossMelee)
        {
            _view = (ViewEnemyBossMelee)viewEnemy;
            _view.Damageable = this;
            
            _view.SetActive(true);
            
            MainTask().Forget();
        }

        public sealed override async UniTaskVoid MainTask()
        {
            var attackTime = 0f;    
            
            while (IsActive)
            {
                Rotate();
                TrackingToMainUnit();
                UpdateView();
                
                if(!IsInView)
                    IsInView = Utility.IsInView(Position);
                
                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                if (IsAttackable)
                {
                    if(!_attackParticle)
                    {
                        _attackParticle = ObjectPoolManager.Instance.GetParticle(ParticleType.ParticleEnemyAttack);
                        var transform = _attackParticle.transform;
                        transform.position = Position + (DirectionToMainUnit * _view.ColliderRange);
                        
                        var particleMain = _attackParticle.main;
                        particleMain.loop = true;
                    }
                    
                    if (attackTime > 1 / AttackSpeed)
                    {
                        Attack();
                        attackTime = 0;
                    }
                    
                    attackTime += Time.deltaTime;
                }
                else
                {
                    if (_attackParticle)
                        _attackParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            if (_attackParticle)
            {
                _attackParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _attackParticle = null;
            }
            IsInView = false;
        }

        private void Attack()
        {
            MainUnitManager.Instance.MainUnitController.Damaged(Power, false);
        }
    }
}