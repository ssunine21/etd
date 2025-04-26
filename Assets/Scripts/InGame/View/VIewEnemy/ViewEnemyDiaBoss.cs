using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewEnemy
{
    public class ViewEnemyDiaBoss : ViewEnemy
    {
        public Transform Tr
        {
            get
            {
                _tr ??= GetComponent<Transform>();
                return _tr;
            }
        }
        private Transform _tr;
        
#if IS_TEST
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            
            // 원의 중심 좌표로 기즈모를 이동
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, ColliderRange);
        }
#endif
    }
}
