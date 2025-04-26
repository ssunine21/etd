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
    public class ControllerBulletLightRocket : ControllerBullet
    {
        private readonly float _moveSpeed = 2f;
        private readonly ViewBulletLightRocket _view;
        
        public ControllerBulletLightRocket(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletLightRocket>())
        {
            _view = (ViewBulletLightRocket)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 from, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            AutoDisable().Forget();
            Position = from;       
            Direction = (enemy.Position - from).normalized;
            
            _view.UpdatePosition(Position);
            _view.TrailRenderer.Clear();
            
            await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);

            var heightArc = Random.Range(-0.1f, 0.1f);
            var x0 = from.x;
            var x1 = enemy.Position.x;

            while (_view.isActiveAndEnabled)
            {
                Move(heightArc, x0, x1, enemy.Position);
                if (TryGetCollidedEnemy(out var target, nonTargets))
                {
                    Attack(target);
                    ShowCollisionParticle();
                    break;
                }

                if (Direction == Vector2.zero)
                    break;

                _view.UpdatePosition(Position);
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            _view.SetActive(false);
        }
        
        private void Move(float heightArc, float x0, float x1, Vector2 targetPosition)
        {
            Position += Direction * _moveSpeed * Time.deltaTime;
            var nextX = Position.x + (Direction * (_moveSpeed * Time.deltaTime)).x;
            var distance = x1 - x0;

            var baseY = Mathf.Lerp(Position.y, targetPosition.y, (nextX - x0) / distance);
            var arc = (heightArc * (nextX - x0) * (nextX - x1)) / (  distance * distance);
            var nextY = baseY + arc;


            var nextPosition = new Vector2(nextX, nextY);
            Position = nextPosition;
        }

        private void ShowCollisionParticle()
        {
            var particleSystem = ObjectPoolManager.Instance.GetParticle(ParticleType.ParticleEnemyAttack);
            var particleMain = particleSystem.main;

            particleSystem.transform.position = Position;
            particleMain.loop = false;
            particleSystem.Play();
        }
    }
}