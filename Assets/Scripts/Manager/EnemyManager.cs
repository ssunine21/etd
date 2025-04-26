using System.Collections.Generic;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller.ControllerEnemy;
using ETD.Scripts.Interface;
using ETD.Scripts.UserData.DataController;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace ETD.Scripts.Manager
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        public UnityAction<ControllerEnemy> onBindDieEnemy;
        public UnityAction<double> onBindDamagedEnemy;
        
        private CancellationTokenSource _cts;

        private bool _isStageChanging;
        private int _currEnemyCount;
        private Camera _camera;

        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
            _camera = Camera.main;
            
            onBindDieEnemy += (_) => DieEnemy();
            StageManager.Instance.onBindChangeStageType += (_,_) => Clear();
            StageManager.Instance.onBindStartStage += (stageType, level) =>
            {
                var enemyDifficultyInfo = stageType switch
                {
                    StageType.Normal => DataController.Instance.stage.GetEnemyDifficultyInfo(level),
                    _ => DataController.Instance.dungeon.GetEnemyDifficultyInfo(stageType, level),
                };
                var enemyCombination = DataController.Instance.enemyCombination.GetEnemyCombination(enemyDifficultyInfo.enemyCombinationIndex);

                if (stageType != StageType.GoldDungeon)
                    _currEnemyCount = 0;
                
                for (var i = 0; i < enemyCombination.enemyTypes.Length; ++i)
                {
                    var count = enemyCombination.counts[i];
                    for(var j = 0; j <count; ++j)
                    {
                        var spawnPosition = stageType switch
                        {
                            StageType.DiaDungeon => new Vector2(0, 3.3f),
                            StageType.GuildRaidDungeon => new Vector2(0, 3.3f),
                            _ => RandomPlacement()
                        };
                        SpawnEnemy(enemyCombination.enemyTypes[i], spawnPosition, enemyDifficultyInfo);
                    }               
                    _currEnemyCount += count;
                }

                _isStageChanging = false;
            };
        }

        public void Clear()
        {
            foreach (var enemy in ObjectPoolManager.Instance.Enemies)
            {
                enemy.viewEnemy.SetActive(false);
            }
            
            ObjectPoolManager.Instance.ClearParticle();
            ObjectPoolManager.Instance.ClearBullet();
            
            _currEnemyCount = 0;
        }

        private void SpawnEnemy(EnemyType type, Vector2 spawnPos, EnemyDifficultyInfo info)
        {
            var enemy = ObjectPoolManager.Instance.GetEnemy(type);

            enemy.Init(info);
            enemy.Position = spawnPos;
            enemy.MainTask().Forget();
        }

        public bool TryOverlapCircle(Vector2 position, float range, out IDamageable target, [CanBeNull] HashSet<IDamageable> nonTargets = null)
        {
            target = null;
            foreach (var enemy in ObjectPoolManager.Instance.Enemies)        
            {
                if(enemy.viewEnemy.isActiveAndEnabled && enemy.IsInView && IsInRange(enemy, position, range))
                {
                    if(nonTargets != null && nonTargets.Contains(enemy)) continue;
                    target = enemy;
                    break;
                }
            }

            return target != null;
        }

        public bool TryOverlapCircleAll(Vector2 position, float range, out HashSet<IDamageable> enemies, [CanBeNull]HashSet<IDamageable> nonTargets = null)
        {
            enemies = null;
            foreach (var enemy in ObjectPoolManager.Instance.Enemies)
            {
                if(enemy.viewEnemy.isActiveAndEnabled && IsInRange(enemy, position, range) && enemy.IsInView)
                {
                    if(nonTargets != null && nonTargets.Contains(enemy)) continue;
                    enemies ??= new HashSet<IDamageable>();
                    enemies.Add(enemy);
                }
            }
            return enemies != null;
        }

        public bool TryGetRandomDamageable(out IDamageable enemy, HashSet<IDamageable> nontargets = null)
        {
            enemy = null;
            foreach (var poolEnemy in ObjectPoolManager.Instance.Enemies)
            {
                if(!poolEnemy.viewEnemy.isActiveAndEnabled || !poolEnemy.IsInView) continue;
                enemy = poolEnemy;
                if(nontargets != null && nontargets.Contains(poolEnemy)) continue;
                enemy = poolEnemy;
                return true;
            }

            return false;
        }

        public bool TryGetNearbyDamageable(Vector2 basePosition, out IDamageable enemy, HashSet<IDamageable> nontargets = null)
        {
            var distance = 999f;
            IDamageable tempEnemy = null;
            
            foreach (var poolEnemy in ObjectPoolManager.Instance.Enemies)
            {
                if(!poolEnemy.viewEnemy.isActiveAndEnabled || !poolEnemy.IsInView) continue;
                if(nontargets != null && nontargets.Contains(poolEnemy)) continue;
                
                var nextDistance = (basePosition - poolEnemy.Position).sqrMagnitude;
                if (nextDistance < distance)
                {
                    distance = nextDistance;
                    tempEnemy = poolEnemy;
                }
            }
            
            enemy = tempEnemy;
            return tempEnemy != null;
        }

        private void DieEnemy()
        {
            if (_isStageChanging) return;
            
            _currEnemyCount--;
            if (StageManager.Instance.PlayingStageType == StageType.GoldDungeon)
            {
                if(_currEnemyCount < 40)
                {
                    _isStageChanging = true;
                    StageManager.Instance.StageClear();
                }
            }
            else
            {
                if (_currEnemyCount <= 0)
                {
                    _isStageChanging = true;
                    StageManager.Instance.StageClear();
                }
            }
        }
        
        private bool IsInRange(ControllerEnemy enemy, Vector2 playerPosition, float range)
        {
            var r = range + enemy.viewEnemy.ColliderRange;
            return (enemy.Position - playerPosition).sqrMagnitude < r * r;
        }

        private Vector2 RandomPlacement()
        {
            if (_camera)
            {
                var random = Random.Range(0, 4);
                var position = random switch
                {
                    0 => new Vector2(Random.Range(-0.2f, 0f), Random.Range(0f, 1.2f)),
                    1 => new Vector2(Random.Range(0f, 1f), Random.Range(1f, 1.2f)),
                    2 => new Vector2(Random.Range(1f, 1.2f), Random.Range(-0.2f, 1f)),
                    3 => new Vector2(Random.Range(-0.2f, 1f), Random.Range(-0.2f, 0f)),
                    _ => Vector2.zero
                };
                return _camera.ViewportToWorldPoint(position);
            }

            return Vector2.zero;
        }
    }
}