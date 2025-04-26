using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALA : ControllerBullet
    {
        private readonly ViewBulletALA _view;
        private LinkedList<ControllerBullet> _lBullet;
        
        public ControllerBulletALA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletALA>())
        {
            _view = (ViewBulletALA)viewBullet;
            _lBullet = new LinkedList<ControllerBullet>();
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Add(enemy);
            
            Shot(enemy.Position, nonTargets).Forget();
        }

        public override async UniTaskVoid Shot(Vector2 endPos, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);

            Position = endPos;       
            _view.UpdatePosition(Position);
            
            AutoDisable().Forget();
            AutoDotAttack().Forget();
            AutoLinkedBulletAttack().Forget();

            while (_view.isActiveAndEnabled)
            {
                if (TryGetCollidedEnemies(out var targets))
                {
                    foreach (var target in targets)
                    {
                        if (_lBullet.Count >= 3)
                            break;
                        
                        var lKey = "ALC";
                        if (ObjectPoolManager.Instance.TryGetBullet(lKey, ProjectorIndex, out var bullet))
                        {
                            ControllerBulletALC alc = (ControllerBulletALC)bullet;
                            _lBullet.AddLast(bullet);

                            var isCritical = Utility.IsProbabilityTrue(DataController.Instance.player.GetCriticalRate(ProjectorIndex));
                            var size = DataController.Instance.elementalCombine.GetSize(lKey);
                            var duration = DataController.Instance.elementalCombine.GetDuration(lKey);
                            
                            alc.EquippedPositionType = EquippedPositionType;
                            alc
                                .SetPower(AttackPower, isCritical)
                                .SetAbility(size, duration, ChainCount);
                            alc.Shot(this, target).Forget();
                        }
                    }
                }

                if(_lBullet.Count > 0)
                {
                    foreach (var lBullet in _lBullet.Where(lBullet => !lBullet.IsActive))
                    {
                        _lBullet.Remove(lBullet);
                        break;
                    }
                }
                
                if (IsLinkedBulletAttackable())
                {
                    if (TryCreateLinkBullet(out var linkBullet))
                        linkBullet.Shot(this, null).Forget();
                }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }
    }
}
