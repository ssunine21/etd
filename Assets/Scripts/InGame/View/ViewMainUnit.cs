using System;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.View
{
    public class ViewMainUnit : View, IRotatable
    {
        public float ColliderRange => colliderRange;
        public Transform[] ProjectorTransforms => projectorTransforms;
        
        [SerializeField] private SpriteRenderer modelSpriteRenderer;
        [SerializeField] private Transform projectorParent;
        [SerializeField] private Transform[] projectorTransforms;
        [SerializeField] private float colliderRange;

        public void Rotate(Vector3 rotation)
        {
            try
            {
                modelSpriteRenderer.transform.rotation = Quaternion.Euler(rotation);
            }
            catch (Exception e)
            {
                var msg = e.Message;
            }
        }
        
        public void ProjectorParentRotate(Vector3 rotation)
        {
            try
            {
                projectorParent.rotation = Quaternion.Euler(rotation);
            }
            catch (Exception e)
            {
                var msg = e.Message;
            }
        }
            
#if IS_TEST
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            
            // 원의 중심 좌표로 기즈모를 이동
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector2.zero, colliderRange);
        }
#endif
    }
}