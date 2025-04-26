using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletALC : ControllerBullet
    {
        private readonly ViewBulletALC _view;

        public ControllerBulletALC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent, InGame.View.View.Get<ViewBulletALC>())
        {
            _view = (ViewBulletALC)viewBullet;
            _view.Laser.Init();

            ChainNode ??= new List<ControllerBullet>();        
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            
            ChainNode.Add(this);
            ChainIndex = ChainNode.Count - 1;

            var projector = MainUnitManager.Instance.ControllerProjectors[ProjectorIndex];
            projector.IsShooting = true;
            
            _view.Laser.StartPosition = unit.Position;
            _view.Laser.EndPosition = enemy.Position;
            _view.Laser.Trigger();
            
            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Add(enemy);
            
            AutoDotAttack().Forget();
            AutoDisable().Forget();
            
            while (enemy.IsActive && unit.IsActive && IsActive)
            {
                if (ChainCount > 0 && ChainNode.Count == ChainIndex + 1)
                {
                    if (EnemyManager.Instance.TryGetNearbyDamageable(enemy.Position, out var nextEnemy, nonTargets))
                    {
                        if (ObjectPoolManager.Instance.TryGetBullet(Key, ProjectorIndex, out var bullet))
                        {
                            var isCritical =
                                Utility.IsProbabilityTrue(
                                    DataController.Instance.player.GetCriticalRate(ProjectorIndex));
                            bullet.ChainNode = ChainNode;

                            bullet
                                .SetPower(AttackPower, isCritical)
                                .SetAbility(Size, Duration, ChainCount - 1)
                                .Shot(enemy, nextEnemy, nonTargets);
                        }
                    }
                }
                
                if (IsDotAttackable())
                {
                    Attack(enemy);
                    
                    if (TryCreateLinkBullet(out var linkBullet))
                        linkBullet.Shot(this, enemy).Forget();
                }
                
                _view.Laser.StartPosition = unit.Position;
                _view.Laser.EndPosition = enemy.Position;
                _view.EndEndParticle.transform.position = enemy.Position;
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
             
            _view.SetActive(false);
            for (var i = ChainNode.Count - 1; i >= ChainIndex; --i)
            {
                ChainNode[i].SetActive(false);
            }
            ChainNode.Remove(this);
            
            projector.IsShooting = false;
        }


        public override async UniTaskVoid Shot(ControllerBullet controllerBullet, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            
            ChainNode.Add(this);
            ChainIndex = ChainNode.Count - 1;

            var projector = MainUnitManager.Instance.ControllerProjectors[ProjectorIndex];
            projector.IsShooting = true;
            
            _view.Laser.StartPosition = controllerBullet.Position;
            _view.Laser.EndPosition = enemy.Position;
            _view.Laser.Trigger();
            
            nonTargets ??= new HashSet<IDamageable>();
            nonTargets.Add(enemy);
            
            AutoDotAttack().Forget();
            AutoDisable().Forget();
            
            while (enemy.IsActive && controllerBullet.IsActive && IsActive)
            {
                if (ChainCount > 0 && ChainNode.Count == ChainIndex + 1)
                {
                    if (EnemyManager.Instance.TryGetNearbyDamageable(enemy.Position, out var nextEnemy, nonTargets))
                    {
                        if(ObjectPoolManager.Instance.TryGetBullet(Key, ProjectorIndex, out var bullet))
                        {
                            var isCritical =
                                Utility.IsProbabilityTrue(
                                    DataController.Instance.player.GetCriticalRate(ProjectorIndex));
                            bullet.ChainNode = ChainNode;

                            bullet
                                .SetPower(AttackPower, isCritical)
                                .SetAbility(Size, Duration, ChainCount - 1)
                                .Shot(enemy, nextEnemy, nonTargets);
                        }
                    }
                }
                
                if (IsDotAttackable())
                {
                    Attack(enemy);
                }
                
                _view.Laser.StartPosition = controllerBullet.Position;
                _view.Laser.EndPosition = enemy.Position;
                _view.EndEndParticle.transform.position = enemy.Position;
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
             
            _view.SetActive(false);
            for (var i = ChainNode.Count - 1; i >= ChainIndex; --i)
            {
                ChainNode[i].SetActive(false);
            }
            ChainNode.Remove(this);
            
            projector.IsShooting = false;
        }
    }
}
