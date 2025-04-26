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
    public class ControllerBulletPDS : ControllerBullet
    {
        public int CircleCount { get; set; } = 4;
        
        private readonly ViewBulletPDC _view;
        
        public ControllerBulletPDS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPDC>())
        {
            _view = (ViewBulletPDC)viewBullet; 
            RotateSpeed = 35f;
            ChainCount = 0;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null){ }

        public override async UniTaskVoid Shot(Vector2 basePosition, HashSet<IDamageable> nonTargets = null)
        {
            for (var i = 0; i < CircleCount; i++)
            {
                var bullet = ObjectPoolManager.Instance.GetBullet("DarkCircle", ProjectorIndex);
                var isCritical = Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

                ((ControllerBulletDarkCircle)bullet).SetActiveEmber(i % 2 == 0);
                
                bullet.IsLinkable = false;
                bullet.EquippedPositionType = EquippedPositionType;
                bullet
                    .SetPower(AttackPower, isCritical)
                    .SetAbility(Size + (i * 0.0408f), Duration, 0);

                bullet.AttackCountPerSecond = AttackCountPerSecond;
                bullet.Shot(Vector2.zero).Forget();
                
                await UniTask.Delay(200, false, PlayerLoopTiming.Update, Cts.Token);
            }
        }
    }
}
