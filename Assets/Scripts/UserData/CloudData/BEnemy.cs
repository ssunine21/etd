using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BEnemy
    {
        public int index;
        public EnemyType enemyType;
        public float moveSpeed;
        public float attackSpeed;
        public float attackRange;
    }
}