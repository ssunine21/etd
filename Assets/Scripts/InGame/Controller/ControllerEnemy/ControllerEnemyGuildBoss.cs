using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller.ControllerBullet;
using ETD.Scripts.InGame.View.ViewEnemy;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ETD.Scripts.InGame.Controller.ControllerEnemy
{
    public class ControllerEnemyGuildBoss : ControllerEnemy
    {
        public static float skillDelayTime;
        
        private readonly ViewEnemyGuildBoss _view;
        private bool _isSkillAttack;
        private const float OriginRotateSpeed = 20f;
        private float _offsetRotateSpeed;
        private ControllerMainUnit _mainUnit;
        
        public ControllerEnemyGuildBoss(CancellationTokenSource cts)
            : base(cts,  InGame.View.View.Get<ViewEnemyGuildBoss>("Enemy"), EnemyType.GuildBossFire)
        {
            _view = (ViewEnemyGuildBoss)viewEnemy;
            _view.Damageable = this;
            
            _view.SetActive(true);
            _view.LazerParent.SetActive(false);
            _mainUnit = MainUnitManager.Instance.MainUnitController;
            
            MainTask().Forget();
            Rotate().Forget();
        }

        public sealed override async UniTaskVoid MainTask()
        {
            const float skillDelay = 8f;
            const float skillDurationTime = 1f;
            var attackTime = 0f;
            var skillDuration = 0f;
            
            while (IsActive)
            {
                TrackingToMainUnit();
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

                    if (skillDelayTime > skillDelay)
                    {
                        _view.LazerParent.SetActive(true);
                        SkillAttack().Forget();
                        _isSkillAttack = true;
                        skillDelayTime = 0;
                        _offsetRotateSpeed = 0f;
                        skillDuration = 0;
                    }

                    skillDelayTime += Time.deltaTime;
                    attackTime += Time.deltaTime;
                }

                if (_isSkillAttack)
                {
                    var t = skillDurationTime * Time.deltaTime;
                    _offsetRotateSpeed = 
                        skillDuration < 0.6f ? Mathf.Lerp(_offsetRotateSpeed, OriginRotateSpeed * 20f, t) : Mathf.Lerp(_offsetRotateSpeed, 0, t);
                    
                    if (skillDuration > skillDurationTime)
                    {
                        _view.LazerParent.SetActive(false);
                        _isSkillAttack = false;
                    }
                    skillDuration += Time.deltaTime;
                }
            }
            IsInView = false;
        }

        public new async UniTaskVoid Rotate()
        {
            var backRotate = new Vector3();
            var innerRotate = new Vector3();
            while (true)
            {
                var innerRotateSpeed = _isSkillAttack ? _offsetRotateSpeed : OriginRotateSpeed;
                backRotate += Vector3.forward * (OriginRotateSpeed * Time.deltaTime);
                innerRotate += Vector3.back * (innerRotateSpeed * Time.deltaTime);
                
                _view.BackgroundView.rotation = Quaternion.Euler(backRotate);
                _view.InnerView.rotation = Quaternion.Euler(innerRotate);
             
                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);   
            }
        }

        public override void Damaged(double damage, bool isCritical)
        {
            base.Damaged(damage, isCritical);
            _view.Tr.DOKill();
            _view.Tr.DOShakePosition(0.06f, 0.1f);
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
        
        public async UniTaskVoid SkillAttack()
        {
            var attackCount = 5;
            while (attackCount > 0)
            {
                _mainUnit?.Damaged(Power * DataController.Instance.guild.GetGuildBossSkilMultiple(), false);
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), false, PlayerLoopTiming.Update, cts.Token);
                attackCount--;
            }
        }
    }
}
