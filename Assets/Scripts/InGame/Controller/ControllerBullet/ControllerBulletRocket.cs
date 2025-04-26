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
    public class ControllerBulletRocket : ControllerBullet
    {
        private readonly ViewBulletRocket _view;
        
        public ControllerBulletRocket(CancellationTokenSource cts, Transform parent) 
            : base(cts, parent,  InGame.View.View.Get<ViewBulletRocket>("Enemy"))
        {
            _view = (ViewBulletRocket)viewBullet;
        }
        
        public override async UniTaskVoid Shot(Vector2 from, Vector2 to, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            Position = from;
            
            _view.UpdatePosition(Position);
            _view.TrailRenderer.Clear();
            
            await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);

            var speed = 5f;
            var heightArc = Random.Range(-3f, 3f);
            
            var x0 = from.x;
            var x1 = to.x;
            Direction = (to - from).normalized;

            while (_view.isActiveAndEnabled)
            {
                var nextX = Position.x + (Direction * (speed * Time.deltaTime)).x;
                var distance = x1 - x0;

                var baseY = Mathf.Lerp(from.y, to.y, (nextX - x0) / distance);
                var arc = (heightArc * (nextX - x0) * (nextX - x1)) / (  distance * distance);
                var nextY = baseY + arc;


                var nextPosition = new Vector2(nextX, nextY);
                Position = nextPosition;
                
                _view.UpdateRotate(LookAt2D(nextPosition - Position));
                _view.UpdatePosition(Position);
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);

                if (TryGetCollidedMainUnit(out var target))
                {
                    target.Damaged(AttackPower, IsCritical);
                    var particleSystem = ObjectPoolManager.Instance.GetParticle(ParticleType.ParticleEnemyAttack);
                    var particleMain = particleSystem.main;
                    particleSystem.transform.position = Position;
                    particleMain.loop = false;
                    
                    _view.SetActive(false);
                }
            }
        }

        public override UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            return new UniTaskVoid();
        }
        
        private Vector3 LookAt2D(Vector2 forward)
        {
            return new Vector3(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
        }
    }
}
