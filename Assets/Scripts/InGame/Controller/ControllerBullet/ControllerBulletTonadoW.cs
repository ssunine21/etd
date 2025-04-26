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
    public class ControllerBulletTonadoW : ControllerBullet
    {
        public float Angle = 0f; // 현재 각도

        private readonly ViewBulletTonadoW _view;
        private const float SpiralRate = 0.9f; // 회오리의 스파이럴 강도
        
        private float _radius = 0.5f; // 현재 반경
        private Vector2 _center;
        
        public ControllerBulletTonadoW(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletTonadoW>())
        {
            _view = (ViewBulletTonadoW)viewBullet;
            CollisionParticle = ObjectPoolManager.Instance.GetParticle(ParticleType.ParticleI);

            MoveSpeed = 180f;
            RotateSpeed = 55f;
        }


        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }
        
        public override async UniTaskVoid Shot(Vector2 basePosition, HashSet<IDamageable> nonTargets = null)
        {
            _view.TrailRenderer.Clear();
            _view.SetActive(true);
            
            nonTargets ??= new HashSet<IDamageable>();

            Angle = 0f;
            _radius = 1f;
            _center = basePosition;
            
            AutoDisable().Forget();
            
            while (_view.isActiveAndEnabled)
            {
                Move();
                Rotate();
                
                if (TryGetCollidedEnemies(out var targets, nonTargets))
                {
                    foreach (var target in targets)
                    {
                        Attack(target);
                        ShowCollisionParticle(Position);
                    }
                    foreach (var target in targets)
                    {
                        nonTargets.Add(target);
                    }
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            
            nonTargets.Clear();
        }
        
        private void Move()
        {
            // 시간에 따라 각도 변화
            Angle += MoveSpeed * Time.deltaTime;

            // 각도를 라디안으로 변환
            var radian = Angle * Mathf.Deg2Rad;

            // 스파이럴 강도에 따라 반경 증가 또는 감소
            _radius += SpiralRate * Time.deltaTime;

            // 새로운 위치 계산
            var x = _center.x + Mathf.Cos(radian) * _radius;
            var y = _center.y + Mathf.Sin(radian) * _radius;

            // 오브젝트 위치 이동
            Position = new Vector2(x, y);
            _view.UpdatePosition(Position);
        }
        
        private void Rotate()
        {
            Rotation += Vector3.back * RotateSpeed * Time.deltaTime;
            _view.UpdateRotate(Rotation);
        }
    }
}
