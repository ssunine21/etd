using System;
using ETD.Scripts.Interface;
using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewEnemy
    {
        public class ViewEnemy : View
        {
            public float ColliderRange => colliderRange;
            public bool IsBoss => isBoss;
            
            public IDamageable Damageable { get; set; }
            
            [SerializeField] private SpriteRenderer hpSpriteRenderer;
            [SerializeField] private SpriteRenderer modelSpriteRenderer;
            
            [SerializeField] private float colliderRange;
            [SerializeField] private bool isBoss;
        
            public void Rotate(Vector3 rotation)
            {
                try
                {
                    modelSpriteRenderer.transform.rotation = Quaternion.Euler(rotation);
                }
                catch (Exception)
                {
                }
            }

            public void UpdatePosition(Vector2 position)
            {
                transform.position = position;
            }

            public void UpdateHp(float scale)
            {
                if (hpSpriteRenderer == null) return;
                
                var hpTransform = Vector3.one;
                hpTransform.x = scale;
                hpSpriteRenderer.transform.localScale = hpTransform;
            }
            
            #if IS_TEST
            private void OnDrawGizmos()
            {
                Gizmos.color = Color.green;
            
                // 원의 중심 좌표로 기즈모를 이동
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(Vector2.zero, ColliderRange);
            }
            #endif
        }
    }