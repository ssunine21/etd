using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.InGame.View.ViewEnemy;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public abstract class ControllerBullet
    {
        public string Key { get; set; }
        public bool IsActive => viewBullet != null && viewBullet.isActiveAndEnabled;
        public ViewBullet ViewBullet => viewBullet;
        
        public List<ControllerBullet> ChainNode { get; set; }

        public float RecoilDistance;
        public bool IsLinkable { get; set; }
        public int ProjectorIndex { get; set; }
        public EquippedPositionType EquippedPositionType { get; set; }
        public int ChainCount { get; set; }
        public float AttackCountPerSecond { get; set; }
        public float ColliderSize { get; private set; }

        protected bool LinkBulletAttacked { get; set; }
        protected float MoveSpeed { get; set; }
        protected float RotateSpeed { get; set; }
        protected int ChainIndex { get; set; }     
        protected float Size { get; private set; }
        protected float Duration { get; private set; }
        protected double AttackPower { get; private set; }
        protected bool IsCritical { get; set; }
        
        private float LinkedBulletAttackSpeed { get; set; }  
        
        protected readonly CancellationTokenSource Cts;
        protected readonly ViewBullet viewBullet;
        
        public Vector2 Position;
        protected Vector3 Rotation;
        protected Vector2 Direction;
        protected ParticleSystem CollisionParticle;
        protected ParticleType ParticleType;

        protected Vector2 LastPosition;
        
        public abstract UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null);
        
        public virtual async UniTaskVoid Shot(Vector2 from, Vector2 to, HashSet<IDamageable> nonTargets = null) { }
        public virtual async UniTaskVoid Shot(Vector2 from, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }
        public virtual async UniTaskVoid Shot(Vector2 endPos, HashSet<IDamageable> nonTargets = null) { }
        public virtual async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }

        protected ControllerBullet(CancellationTokenSource cts, Transform parent, ViewBullet view)
        {
            Cts = cts;
            
            if(view)
            {
                viewBullet = view;
                viewBullet.transform.SetParent(parent);
            }
        }

        private bool _isLinkedBulletAttackable;
        protected bool IsLinkedBulletAttackable()
        {
            if(Utility.IsInView(Position))
            {
                if (_isLinkedBulletAttackable)
                {
                    _isLinkedBulletAttackable = false;
                    AutoLinkedBulletAttack().Forget();
                    return true;
                }
            }
            return false;
        }

        private bool _isDotAttackable = true;
        protected bool IsDotAttackable()
        {
            if (_isDotAttackable)
            {
                _isDotAttackable = false;
                AutoDotAttack().Forget();
                return true;
            }
            return false;
        }

        private float _linkedBulletAttackTime;
        protected async UniTaskVoid AutoLinkedBulletAttack()
        {
            _linkedBulletAttackTime = 0;
            _isLinkedBulletAttackable = false;
            
            while (viewBullet.isActiveAndEnabled)
            {
                if (_linkedBulletAttackTime >= 1 / LinkedBulletAttackSpeed)
                {
                    _isLinkedBulletAttackable = true;
                    _linkedBulletAttackTime = 0;
                    break;
                }
                
                _linkedBulletAttackTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }

        protected async UniTaskVoid AutoDotAttack()
        {
            var second = 1 / AttackCountPerSecond;
            var token = Cts.Token;
            try
            {
                var attackTimeTask = UniTask.Delay(TimeSpan.FromSeconds(second), cancellationToken: token);
                var disableTask = UniTask.WaitUntil(() => !viewBullet.isActiveAndEnabled, cancellationToken: token);

                await UniTask.WhenAny(attackTimeTask, disableTask);
                _isDotAttackable = true;
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected async UniTaskVoid AutoDisable()
        {
            var duration = Duration;
            var token = Cts.Token;
            try
            {
                var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
                var disableTask = UniTask.WaitUntil(() => !viewBullet.isActiveAndEnabled, cancellationToken: token);
                await UniTask.WhenAny(timeoutTask, disableTask);
                
                SetActive(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void SetActive(bool flag)
        {
            viewBullet.SetActive(flag);
        }

        public ControllerBullet SetPower(double power, bool isCritical)
        {
            AttackPower = power;
            IsCritical = isCritical;
            return this;
        }
        
        public virtual ControllerBullet SetAbility(BulletAbility bulletAbility)
        {
            SetPower(bulletAbility.Power, bulletAbility.IsCritical);
            SetAbility(bulletAbility.Size, bulletAbility.DurationTime, bulletAbility.ChainCount);
            return this;
        }

        public virtual ControllerBullet SetAbility(float size = 0, float duration = 0, int chainCount = 0)
        {
            Size = size;
            Duration = duration;
            ChainCount = chainCount;

            var equipmentPositionType = GetEquippedPositionType();

            MoveSpeed = DataController.Instance.elementalCombine.GetMoveSpeed(Key);
            AttackCountPerSecond = DataController.Instance.player.GetAttackCountForSecond(ProjectorIndex, equipmentPositionType);
            LinkedBulletAttackSpeed = DataController.Instance.player.GetAttackSpeed(ProjectorIndex, EquippedPositionType.Link);
            
            ColliderSize = viewBullet.radius * Size;
            viewBullet.UpdateSize(Size);

            return this;
        }
        
        protected bool TryCreateLinkBullet(out ControllerBullet linkBullet)
        {
            if (IsLinkable)
            {
                var linkKey = DataController.Instance.player.GetElementalKey(ProjectorIndex, EquippedPositionType.Link);
                if (ObjectPoolManager.Instance.TryGetBullet(linkKey, ProjectorIndex, out var bullet))
                {
                    var attackPower = DataController.Instance.player.GetAttackPower(ProjectorIndex)
                                      * DataController.Instance.elementalCombine.GetAttackCoefficient(bullet.Key);
                    var isCritical =
                        Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

                    var size = DataController.Instance.player.GetBulletSize(ProjectorIndex, EquippedPositionType.Link);
                    var durationTime =
                        DataController.Instance.player.GetBulletDurationTime(ProjectorIndex, EquippedPositionType.Link);
                    var chainCount = (int)DataController.Instance.attribute.GetTagValueOrDefault(TagType.Chain, ProjectorIndex);

                    bullet
                        .SetPower(attackPower, isCritical)
                        .SetAbility(size, durationTime, chainCount);

                    bullet.EquippedPositionType = EquippedPositionType.Link;
                    linkBullet = bullet;
                    return true;
                }
            }

            linkBullet = null;
            return false;
        }

        protected bool TryCopyBullet(out ControllerBullet copy)
        {
            return TryCopyBullet(out copy, Key);
        }
        
        protected bool TryCopyBullet(out ControllerBullet copy, string key)
        {
            copy = null;
            if (ObjectPoolManager.Instance.TryGetBullet(key, ProjectorIndex, out var bullet))
            {
                var isCritical =
                    Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

                bullet
                    .SetPower(AttackPower, isCritical)
                    .SetAbility(Size, Duration, ChainCount);

                bullet.IsLinkable = IsLinkable;
                bullet.MoveSpeed = MoveSpeed == 0 ? bullet.MoveSpeed : MoveSpeed;
                bullet.AttackCountPerSecond = AttackCountPerSecond;
                bullet.LinkedBulletAttackSpeed = LinkedBulletAttackSpeed;
                bullet.EquippedPositionType = EquippedPositionType;
                bullet.ParticleType = ParticleType;
                copy = bullet;
            }
            return copy != null;
        }
        
        protected void Attack(IDamageable target)
        {
            var totalPower = AttackPower * (IsCritical ? DataController.Instance.player.GetCriticalDamage(ProjectorIndex) : 1);
            if (target.IsActive)
            {
                target.Damaged(totalPower, IsCritical);
                var index = 
                    Mathf.Clamp(ProjectorIndex * 3 + (int)GetEquippedPositionType(), 0, DataController.Instance.player.dpsContainer.Length - 1);
          
                DataController.Instance.player.dpsContainer[index] += totalPower;
                DataController.Instance.player.totalDps += totalPower;
            }
        }
        
        protected bool TryGetCollidedMainUnit(out IDamageable target)
        {
            target = MainUnitManager.Instance.OverlapCircle(Position, ColliderSize);
            return target != null;
        }
        
        protected bool TryGetCollidedEnemy(out IDamageable target,  Vector2 basePosition, float range, HashSet<IDamageable> nonTargets = null)
        {
            return EnemyManager.Instance.TryOverlapCircle(basePosition, range, out target, nonTargets);
        }
        
        protected bool TryGetCollidedEnemy(out IDamageable target, HashSet<IDamageable> nonTargets = null)
        {
            target = null;
            nonTargets ??= new HashSet<IDamageable>();
            var from = LastPosition;
            var to = Position;
            var dir = to - from;
            var dist = dir.magnitude;

            if (dist > 0f)
            {
                var hits = Physics2D.CircleCastAll(from, ColliderSize, dir, dist);
                foreach (var hit in hits)
                {
                    if (hit.collider.TryGetComponent<ViewEnemy>(out var viewEnemy))
                    {
                        if (!nonTargets.Contains(viewEnemy.Damageable))
                        {
                            target = viewEnemy.Damageable;
                            break;
                        }
                    }
                }
            }

            LastPosition = Position;
            return target != null;
        }

        protected bool TryGetCollidedEnemies(Vector2 from, Vector2 to, float radius, out HashSet<IDamageable> targets, HashSet<IDamageable> nonTargets = null)
        {
            var tempTargets = new HashSet<IDamageable>();
            nonTargets ??= new HashSet<IDamageable>();
            var dir = to - from;
            var dist = dir.magnitude;

            if (dist > 0f)
            {
                var hits = Physics2D.CircleCastAll(from, radius, dir, dist);
                foreach (var hit in hits)
                {
                    if (hit.collider.TryGetComponent<ViewEnemy>(out var viewEnemy))
                    {
                        if (!nonTargets.Contains(viewEnemy.Damageable))
                            tempTargets.Add(viewEnemy.Damageable);
                    }
                }
            }
            else
            {
                return EnemyManager.Instance.TryOverlapCircleAll(from, radius, out targets, nonTargets);
            }
            
            LastPosition = Position;
            targets = tempTargets.Count > 0 ? tempTargets : null;
            return targets != null;
        }
        
        protected bool TryGetCollidedEnemies(out HashSet<IDamageable> targets, HashSet<IDamageable> nonTargets = null)
        {
            return TryGetCollidedEnemies(out targets, ColliderSize, nonTargets);
        }
        
        protected bool TryGetCollidedEnemies(out HashSet<IDamageable> targets, float range, HashSet<IDamageable> nonTargets = null)
        {
            EnemyManager.Instance.TryOverlapCircleAll(Position, range, out targets, nonTargets);
            
            return targets != null;
        }
        
        protected bool TryGetCollidedEnemies(out HashSet<IDamageable> targets, Vector2 basePosition, float range, HashSet<IDamageable> nonTargets = null)
        {
            return EnemyManager.Instance.TryOverlapCircleAll(basePosition, range, out targets, nonTargets);
        }

        protected void ShowCollisionParticle(Vector2 pos)
        {
            if (CollisionParticle == null) return;
            if (CollisionParticle.isPlaying) return;
            
            CollisionParticle.gameObject.SetActive(true);
            var transform = CollisionParticle.transform;
            transform.position = pos;
            if (Direction != Vector2.zero)
                transform.forward = Direction;
            CollisionParticle.Play();
        }
        
        protected void ShowParticle(Vector2 pos)
        {
            var particle =  ObjectPoolManager.Instance.GetParticle(ParticleType);
            particle.transform.position = pos;
            if (Direction != Vector2.zero)
                particle.transform.forward = Direction;
        }

        protected EquippedPositionType GetEquippedPositionType()
        {
            return EquippedPositionType;
        }

        private Vector2 _aroundMoveDirection;
        protected void MoveAroundToTarget(Vector2 to)
        {
            Direction = (to - Position).normalized;
            if (Direction != Vector2.zero)
            {
                _aroundMoveDirection = Vector2.Lerp(_aroundMoveDirection, Direction, 2f * Time.deltaTime).normalized;
                Position += _aroundMoveDirection * MoveSpeed * Time.deltaTime;   
                viewBullet.UpdatePosition(Position);
            }
        }

        protected void MoveToDicrect()
        {
            Position += Direction * MoveSpeed * Time.deltaTime;     
            viewBullet.UpdatePosition(Position);
        }

        protected void MoveToTarget(Vector2 to)
        {
            Direction = (to - Position).normalized;
            MoveToDicrect();
        }
        
        protected void Rotate()
        {
            Rotation += Vector3.back * RotateSpeed * Time.deltaTime;
            viewBullet.UpdateRotate(Rotation);
        }

        protected bool IsReachedTarget(Vector2 to)
        {
            return Vector2.Distance(Position, to) < 0.1f;
        }

        protected void PullEnemy(IDamageable enemy, float pullForce = 0.2f, float duration = 0.2f)
        {
            var pullDir = (Position - enemy.Position).normalized;
            var targetPos = enemy.Position + pullDir * pullForce;
            enemy.View.transform.DOMove(targetPos, duration).SetEase(Ease.OutSine).OnComplete(() =>
            {
                enemy.Position = enemy.View.transform.position;
            });
        }
    }
}