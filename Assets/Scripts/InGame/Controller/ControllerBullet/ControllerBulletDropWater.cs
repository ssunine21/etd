using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletDropWater: ControllerBullet
    {
        private readonly ViewBulletDropWater _view; 

        private bool _isDropped;
        private Vector2 dropPosition;
        
        public ControllerBulletDropWater(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletDropWater>())
        {
            _view = (ViewBulletDropWater)viewBullet;
            MoveSpeed = 15f;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 endPos, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            _view.Model.SetActive(false);

            _isDropped = false;
            dropPosition = endPos;
            Position = new Vector2(endPos.x + 1, endPos.y + 4);
            Direction = (endPos - Position).normalized;

            _view.UpdatePosition(Position);
            _view.TrailRenderer.Clear();

            AutoDisable().Forget();

            while (!_isDropped && _view.isActiveAndEnabled)
            {
                Move();
                _view.UpdatePosition(Position);
                
                if (Vector2.Distance(Position, dropPosition) <= 0.4f)
                {
                    _isDropped = true;
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }

            AutoDotAttack().Forget();
            
            _view.Model.SetActive(true);
            while (_view.isActiveAndEnabled)
            {
                if (IsDotAttackable())
                {
                    if (TryGetCollidedEnemies(out var targets))
                    {
                        foreach (var target in targets)
                        {
                            Attack(target);
                        }
                    }
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
        
        private void Move()
        {
            Position += Direction * MoveSpeed * Time.deltaTime;
        }
    }
}
