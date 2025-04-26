using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller.ControllerBullet;
using ETD.Scripts.InGame.Controller.ControllerEnemy;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Manager
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        public UnityAction OnClearBullet;

        public List<ControllerEnemy> Enemies
        {
            get
            {
                _enemies ??= new List<ControllerEnemy>();
                return _enemies;
            }
        }

        public Dictionary<string, List<ControllerBullet>> Bullets
        {
            get
            {
                _bulletDic ??= new Dictionary<string, List<ControllerBullet>>();
                return _bulletDic;
            }
        }
        
        private static CancellationTokenSource _cts;

        private Dictionary<string, List<ControllerBullet>> _bulletDic;
        private Dictionary<ParticleType, Queue<ParticleSystem>> _particleDic;
        private Dictionary<EnemyType, List<ControllerEnemy>> _enemyDic;
        private Dictionary<ObjectPoolType, Transform> _objectParentDic;

        private List<ControllerEnemy> _enemies;
        
        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
        }
        
        public ControllerEnemy GetEnemy(EnemyType enemyType)
        {
            _enemyDic ??= new Dictionary<EnemyType, List<ControllerEnemy>>();
            if (!_enemyDic.ContainsKey(enemyType))
                _enemyDic.Add(enemyType, new List<ControllerEnemy>());

            var enemy = _enemyDic[enemyType].Find(enemy => !enemy.IsActive);
            if (enemy != null)
            {
                return enemy;
            }

            enemy = CreateEnemy(enemyType);
            _enemyDic[enemyType].Add(enemy);
            Enemies.Add(enemy);

            return enemy;
        }

        public bool TryGetBullet(string key, int projectorIndex, out ControllerBullet bullet)
        {
            if (string.IsNullOrEmpty(key) || _cts == null)
            {
                bullet = null;
                return false;
            }

            _bulletDic ??= new Dictionary<string, List<ControllerBullet>>();
            bullet = _bulletDic.TryGetValue(key, out var bullets) 
                ? bullets.Find(bullet => !bullet.IsActive) ?? CreateBullet(key) 
                : CreateBullet(key);

            bullet.ProjectorIndex = projectorIndex;
            
            return true;
        }
        
        public ControllerBullet GetBullet(string key, int projectorIndex)
        {
            if (string.IsNullOrEmpty(key) || _cts == null)
            {
                return null;
            }

            _bulletDic ??= new Dictionary<string, List<ControllerBullet>>();
            if (!_bulletDic.ContainsKey(key))
                _bulletDic.Add(key, new List<ControllerBullet>());

            var bullet = _bulletDic[key].Find(bullet => !bullet.IsActive);
            if (bullet == null)
            {
                bullet = CreateBullet(key);
                _bulletDic[key].Add(bullet);
            }
            bullet.ProjectorIndex = projectorIndex;
            return bullet;
        }

        public ParticleSystem GetParticle(ParticleType particleType)
        {
            _particleDic ??= new Dictionary<ParticleType, Queue<ParticleSystem>>();
            if (!_particleDic.ContainsKey(particleType))
                _particleDic.Add(particleType, new Queue<ParticleSystem>());

            if (_particleDic[particleType].Count == 0)
            {
                _particleDic[particleType].Enqueue(CreateParticle(particleType));
            }

            var createParticle = _particleDic[particleType].Dequeue();
            createParticle.gameObject.SetActive(true);
            createParticle.Play();

            StartCoroutine(ReturnWhenFinished(particleType, createParticle));
            return createParticle;
        }

        private IEnumerator ReturnWhenFinished(ParticleType type, ParticleSystem ps)
        {
            yield return new WaitWhile(() => ps.IsAlive(true));
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            ps.gameObject.SetActive(false);
            _particleDic[type].Enqueue(ps);
        }

        public void ChangeBullet()
        {
            ClearBullet();
        }

        private ControllerEnemy CreateEnemy(EnemyType enemyType)
        {
            ControllerEnemy enemy = enemyType switch
            {
                EnemyType.Melee => new ControllerEnemyMelee(_cts),
                EnemyType.Range => new ControllerEnemyRanged(_cts),
                EnemyType.BossMelee => new ControllerEnemyBossMelee(_cts),
                EnemyType.DiaBoss => new ControllerEnemyDiaBoss(_cts),
                EnemyType.GuildBossFire => new ControllerEnemyGuildBoss(_cts),
                _ => new ControllerEnemyMelee(_cts)
            };

            enemy.viewEnemy.transform.SetParent(GetParent(ObjectPoolType.Enemy));
            return enemy;
        }
        
        public ControllerBullet CreateBullet(string key)
        {
            if (_cts == null) return null;
            
            _bulletDic ??= new Dictionary<string, List<ControllerBullet>>();
            if (!_bulletDic.ContainsKey(key))
                _bulletDic.Add(key, new List<ControllerBullet>());
            
            var bullet = DataController.Instance.elemental.GetBullet(key, _cts, GetParent(ObjectPoolType.Bullet));
            bullet.Key = key;
            
            _bulletDic[key].Add(bullet);
            
            return bullet;
        }

        private ParticleSystem CreateParticle(ParticleType particleType)
        {
            var particle = Instantiate(ResourcesManager.ParticleSystems[particleType], GetParent(ObjectPoolType.Particle));
            return particle;
        }

        private Transform GetParent(ObjectPoolType type)
        {
            _objectParentDic ??= new Dictionary<ObjectPoolType, Transform>();
            if(!_objectParentDic.ContainsKey(type))
            {
                _objectParentDic.Add(type, new GameObject($"@ObjectPool_{type.ToString()}_Parent").transform);
            }

            return _objectParentDic[type];
        }

        public void ClearParticle()
        {
            if (_particleDic == null) return;
            foreach (var particle in _particleDic.SelectMany(particles => particles.Value))
            {
                particle.gameObject.SetActive(false);
            }
        }

        public void ClearBullet()
        {
            if (_bulletDic == null) return;
            foreach (var bullet in _bulletDic.Values.SelectMany(bullets => bullets))
            {
                bullet.SetActive(false);
            }
            OnClearBullet?.Invoke();
        }
    }
}