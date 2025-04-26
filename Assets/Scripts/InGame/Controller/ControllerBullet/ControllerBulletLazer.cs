using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletLazer : ControllerBullet
    {
        public UnityAction<IDamageable> onCollisionEnter;
        
        private readonly ViewBulletLazer _view;
        private readonly SpriteAnimation _spriteAnimation;

        public ControllerBulletLazer(CancellationTokenSource cts, Transform parent) 
            : base(cts, parent, View.View.Get<ViewBulletLazer>())
        {
            _view = (ViewBulletLazer)viewBullet;
            _spriteAnimation = _view.GetComponent<SpriteAnimation>();
            _view.onCollisionEnter += arg0 => onCollisionEnter?.Invoke(arg0);
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);
            
            if(_spriteAnimation != null)
                _spriteAnimation.StartAnimation();
        }

        public void SetRotation(Vector3 value)
        {
            _view.transform.rotation = Quaternion.Euler(value);
        }
    }
}