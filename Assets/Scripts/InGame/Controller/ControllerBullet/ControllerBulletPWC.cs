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
    public class ControllerBulletPWC : ControllerBullet
    {
        private readonly ViewBulletPWC _view;
        private const string SubKey = "TonadoWWW";

        public ControllerBulletPWC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPWC>())
        {
            _view = (ViewBulletPWC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 unitPosition, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            AutoDotAttack().Forget();
            
            var baseAttackSpeed = DataController.Instance.player.GetAttackSpeed(ProjectorIndex, EquippedPositionType.Passive);
            AttackCountPerSecond = baseAttackSpeed;
            
            while (_view.isActiveAndEnabled)
            {
                if (IsDotAttackable())
                {
                        var basePosition = Utility.RandomPositionInView();
                        SpawnProjectile(basePosition);
                        
                        var delay = Random.Range(20, 70);
                        await UniTask.Delay(delay, false, PlayerLoopTiming.Update, Cts.Token);
                }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            _view.SetActive(false);
        }

        private void SpawnProjectile(Vector2 basePosition)
        {
            var bullet = ObjectPoolManager.Instance.GetBullet(SubKey, ProjectorIndex);
            var isCritical = Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

            bullet.IsLinkable = false;
            bullet.EquippedPositionType = EquippedPositionType;
            bullet
                .SetPower(AttackPower, isCritical)
                .SetAbility(Size, Duration, 0);

            bullet.AttackCountPerSecond = DataController.Instance.player.GetAttackCountForSecond(ProjectorIndex, EquippedPositionType.Passive);
            bullet.Shot(basePosition).Forget();
        }
    }
}
