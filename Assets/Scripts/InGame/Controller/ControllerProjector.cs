using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller
{
    public class ControllerProjector : IDamageable
    {
        public Vector2 Position
        {
            get => _view.Position;
            set => _view.Position = value;
        }

        public bool IsActive => _view.isActiveAndEnabled;
        public View.View View => _view;
        public bool IsShooting { get; set; }

        private readonly int _index;

        private readonly CancellationTokenSource _cts;
        private readonly ViewProjector _view;

        private const float RotateSpeed = 18f;

        private readonly Vector2 _originLocalPosition;
        private Vector3 _rightRotate;
        private Vector3 _leftRotate;

        private ControllerBullet.ControllerBullet _passiveBullet;
        
        private readonly Dictionary<EquippedPositionType, bool> _attackAvailabilityDic = new();
        private Sequence _recoilSequnce;

        public ControllerProjector(CancellationTokenSource cts, int index)
        {
            _cts = cts;
            _view = InGame.View.View.Get<ViewProjector>();
            _view.SetActive(true);
            _index = index;
            _originLocalPosition = _view.transform.localPosition;

            IsShooting = false;

            DataController.Instance.player.OnBindChangedElemental += UpdateBullet;
            DataController.Instance.player.OnBindChangedRune += UpdateBullet;
            StageManager.Instance.onBindChangeStageType += (_, _) => _passiveBullet = null;
            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseElementalUnit] += (_) => UpdateUnit();

            ObjectPoolManager.Instance.OnClearBullet += () => _passiveBullet = null;
            
            MainTask().Forget();
        }

        private async UniTaskVoid MainTask()
        {
            await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
            await UniTask.WaitUntil(() => DataController.IsInit);
            
            
            UpdateUnit();
            
            UpdateAttackAvailability(EquippedPositionType.Active, _cts.Token).Forget();
            UpdateAttackAvailability(EquippedPositionType.Passive, _cts.Token).Forget();

            AttackTask(EquippedPositionType.Active, _cts.Token).Forget();
            AttackTask(EquippedPositionType.Passive, _cts.Token).Forget();

            ChangeUnitColor(_index);
        }

        private async UniTaskVoid AttackTask(EquippedPositionType positionType, CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                var count = DataController.Instance.attribute.GetTagValueOrDefault(TagType.Projectile, _index, 0);
                if (_attackAvailabilityDic.GetValueOrDefault(positionType, false) && GameManager.Instance.IsPlaying)
                {
                    if(positionType == EquippedPositionType.Active)
                    {
                        if (EnemyManager.Instance.TryGetNearbyDamageable(Position, out var enemy0))
                        {
                            Shot(enemy0);
                        }

                        var nonTarget = new HashSet<IDamageable>();
                        for (var i = 0; i < count; ++i)
                        {
                            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
                            if (EnemyManager.Instance.TryGetRandomDamageable(out var enemy1, nonTarget))
                            {
                                nonTarget.Add(enemy1);
                                Shot(enemy1);
                            }
                        }
                    }
                    else if (positionType == EquippedPositionType.Passive)
                        PassiveShot();

                    _attackAvailabilityDic[positionType] = false;
                }
                
                await UniTask.Yield(token);
            }
        }
        

        private async UniTaskVoid UpdateAttackAvailability(EquippedPositionType positionType, CancellationToken token)
        {
            var player = DataController.Instance.player;

            while (!token.IsCancellationRequested)
            {
                
                var attackSpeed = player.GetAttackSpeed(_index, positionType);
                if (attackSpeed <= 0)
                {
                    _attackAvailabilityDic[positionType] = false;
                    await UniTask.Yield(token);
                    continue;
                }
                
                var interval = 1f / player.GetAttackSpeed(_index, positionType);
                
                await UniTask.WaitUntil(() => !_attackAvailabilityDic.GetValueOrDefault(positionType, false), cancellationToken: token);
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
                
                _attackAvailabilityDic[positionType] = true;
            }
        }
        
        private void Shot(IDamageable enemy)
        {
            var bulletKey = DataController.Instance.player.GetElementalKey(_index, EquippedPositionType.Active);
            if (ObjectPoolManager.Instance.TryGetBullet(bulletKey, _index, out var activeBullet))
            {
                SetAbility(activeBullet, EquippedPositionType.Active);
                activeBullet.Shot(this, enemy).Forget();

                PlayRecoil(enemy.Position - Position, activeBullet.RecoilDistance);
            }
        }

        private void PassiveShot()
        {
            if (_passiveBullet == null)
            {
                var bulletKey = DataController.Instance.player.GetElementalKey(_index, EquippedPositionType.Passive);
                if (ObjectPoolManager.Instance.TryGetBullet(bulletKey, _index, out var passiveBullet))
                {
                    _passiveBullet = passiveBullet;

                    SetAbility(passiveBullet, EquippedPositionType.Passive);
                    passiveBullet.Shot(Position).Forget();
                }
            }
        }
        
        public void Damaged(double damage, bool isCritical) { }

        public void SetPosition(Vector2 position)
        {
            _view.Position = position;
        }

        private void PlayRecoil(Vector2 shootDirection, float recoilDistance)
        {
            _recoilSequnce?.Kill(true);
            var recoilOffset = -shootDirection.normalized * recoilDistance;
            var rotOffset = new Vector3(0f, 0f, 300f * recoilDistance); // Z축 기준 회전 (2D 기준)
            var recoilDuration = 0.5f;

            _recoilSequnce = DOTween.Sequence()
                .Append(_view.transform.DOBlendableMoveBy(recoilOffset, recoilDuration).SetEase(Ease.OutExpo))
                .Join(_view.transform.DOBlendableLocalRotateBy(rotOffset, recoilDuration, RotateMode.FastBeyond360).SetEase(Ease.OutExpo)) // 빠르게 튀듯이
                .Append(_view.transform.DOLocalMove(_originLocalPosition, 0.15f).SetEase(Ease.InOutQuad))
                .Join(_view.transform.DOLocalRotate(Vector3.zero, 0.15f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad)); // 천천히 돌아옴;
        }

        private void UpdateBullet(int index)
        {
            ChangeUnitColor(index);
            if (index == _index)
            {          
                ObjectPoolManager.Instance.ChangeBullet();
            }
        }

        private void ChangeUnitColor(int projectorIndex)
        {
            if (projectorIndex != _index) return;
            var newColor = DataController.Instance.player.GetProjectorUnitColor(projectorIndex);
            foreach (var spriteRenderer in _view.UnitSpriteRenderer)
                spriteRenderer.color = newColor;
        }
        
        private void SetAbility(ControllerBullet.ControllerBullet bullet, EquippedPositionType equippedPositionType)
        {
            var bulletAbility = new BulletAbility(bullet.Key)
            {
                Power = DataController.Instance.player.GetAttackPower(_index) * DataController.Instance.elementalCombine.GetAttackCoefficient(bullet.Key),
                IsCritical = Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(_index)),
                Size = DataController.Instance.player.GetBulletSize(_index, equippedPositionType),
                DurationTime = DataController.Instance.player.GetBulletDurationTime(_index, equippedPositionType),
                ChainCount = (int)DataController.Instance.attribute.GetTagValueOrDefault(TagType.Chain, _index)
            };
            
            bullet.IsLinkable = true;
            bullet.EquippedPositionType = equippedPositionType;
            
            bullet.SetAbility(bulletAbility);
        }
        
        public void SetParent(Transform parent)
        {
            _view.transform.SetParent(parent);
        }
        
        private void Rotate()
        {
            _rightRotate += Vector3.back * RotateSpeed * Time.deltaTime;
            _leftRotate += Vector3.forward * RotateSpeed * Time.deltaTime;
        }

        private void UpdateView()
        {
            _view.Rotate(_leftRotate);
            _view.SubRotate(_rightRotate);
        }

        private void UpdateUnit()
        {
            for (var i = 1; i < _view.UnitSpriteRenderer.Length; ++i)
            {
                _view.UnitSpriteRenderer[i].enabled = !DataController.Instance.upgrade.IsLockElemental(_index, i);
            }
        }
    }
}