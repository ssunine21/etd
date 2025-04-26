using System;
using System.Linq;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataEnemyCombination enemyCombination;
    }

    [Serializable]
    public class DataEnemyCombination
    {
        public BEnemyCombination[] BData => CloudData.CloudData.Instance.bEnemyCombinations;
        
        public BEnemyCombination GetEnemyCombination(int index)
        {
            return BData[index];
        }

        public int GetEnemyCount(int index)
        {
            return BData[Mathf.Clamp(index, 0, BData.Length - 1)].counts.Sum();
        }
    }
}