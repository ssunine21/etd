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
    public class ControllerBulletPFC : ControllerBullet
    {
        private readonly ViewBulletPFC _view;
        private const string SubKey = "DropFire";

        public ControllerBulletPFC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPFC>())
        {
            _view = (ViewBulletPFC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 unitPosition, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            AutoDotAttack().Forget();
            
            while (_view.isActiveAndEnabled)
            {
                if (IsDotAttackable())
                {
                        var baseAttackSpeed =
                            DataController.Instance.player.GetAttackSpeed(ProjectorIndex, EquippedPositionType.Passive);
                        var offsetAttackSpeed = baseAttackSpeed * 0.5f;
                        AttackCountPerSecond = baseAttackSpeed + Random.Range(-offsetAttackSpeed, offsetAttackSpeed);

                        var basePosition = Utility.RandomPositionInView();
                        SpawnProjectile(basePosition);
                }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            _view.SetActive(false);
        }

        private void SpawnProjectile(Vector2 basePosition)
        {
            var bullet = ObjectPoolManager.Instance.GetBullet(SubKey, ProjectorIndex);
            var isCritical =
                Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));

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
