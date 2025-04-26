using System;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using JetBrains.Annotations;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataEnemy enemy;
    }

    [Serializable]
    public class DataEnemy
    {
        private BEnemy[] BData => CloudData.CloudData.Instance.bEnemies;

        [CanBeNull] public BEnemy GetData(EnemyType type)
        {
            return BData.FirstOrDefault(x => x.enemyType == type);
        }
        
        public float GetAttackRange(EnemyType type)
        {
            var data = GetData(type);
            return data?.attackRange ?? 0;
        }

        public float GetAttackSpeed(EnemyType type)
        {
            var data = GetData(type);
            return data?.attackSpeed ?? 0;
        }
        
        public float GetMoveSpeed(EnemyType type)
        {
            var data = GetData(type);
            return data?.moveSpeed ?? 0;
        }
    }
}