using System;
using ETD.Scripts.Interface;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBulletLazer : ViewBullet
    {
        public UnityAction<IDamageable> onCollisionEnter;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                if (other.transform.TryGetComponent(out ViewEnemy.ViewEnemy enemy))
                {
                    if (enemy.isActiveAndEnabled)
                        onCollisionEnter?.Invoke(enemy.Damageable);
                }
            }
        }
    }
}