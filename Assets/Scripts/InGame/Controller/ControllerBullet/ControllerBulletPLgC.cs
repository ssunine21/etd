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
    public class ControllerBulletPLgC : ControllerBullet
    {
        private readonly ViewBulletPLgC _view;
        
        public ControllerBulletPLgC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPLgC>())
        {
            _view = (ViewBulletPLgC)viewBullet; 
            RotateSpeed = 35f;
            ChainCount = 0;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null){ }

        public override async UniTaskVoid Shot(Vector2 basePosition, HashSet<IDamageable> nonTargets = null)
        {
            _view.UpdateSize(0.23f);
            _view.SetActive(true);

            var count = DataController.Instance.attribute.GetTagValueOrDefault(TagType.Projectile, ProjectorIndex, 1);
            // 360도/12개 = 30도 각도마다 오브젝트 배치
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count; // 각도를 라디안으로 계산

                // 오브젝트의 위치를 계산
                var x = Mathf.Cos(angle) * 8.1f;
                var y = Mathf.Sin(angle) * 8.1f;

                // 위치 설정
                var objectPosition = new Vector3(x, y, 0f);
                
                if (ObjectPoolManager.Instance.TryGetBullet("LightingBall", ProjectorIndex, out var bullet))
                {
                    var transform = bullet.ViewBullet.transform;
                    transform.SetParent(_view.Model.transform);
                    transform.localPosition = objectPosition;
                    
                    bullet.EquippedPositionType = EquippedPositionType;
                    bullet
                        .SetPower(AttackPower, IsCritical)
                        .SetAbility(Size, -1, ChainCount)
                        .Shot(transform.position).Forget();
                }
            }

            while (_view.isActiveAndEnabled)
            {
                Rotate();
                _view.UpdateRotate(Rotation);
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }
        
        private void Rotate()
        {
            Rotation += Vector3.back * RotateSpeed * Time.deltaTime;
        }
    }
}
