using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BEnemyCombination
    {
        public int index;
        public EnemyType[] enemyTypes;
        public int[] counts;
    }
}