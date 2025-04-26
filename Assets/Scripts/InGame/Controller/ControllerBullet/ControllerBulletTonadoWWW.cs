using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletTonadoWWW: ControllerBullet
    {
        private readonly ViewBulletTonadoWWW _view; 
        
        public ControllerBulletTonadoWWW(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletTonadoWWW>())
        {
            _view = (ViewBulletTonadoWWW)viewBullet;
            RotateSpeed = 125f;
        }
        
        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 endPos, HashSet<IDamageable> nonTargets = null)
        {
            FadeAndActive(true);

            Position = endPos;
            _view.UpdatePosition(Position);

            AutoDisable().Forget();
            AutoDotAttack().Forget();
            
            while (_view.isActiveAndEnabled)
            {
                Rotate();
                
                if (IsDotAttackable())
                {
                    if (TryGetCollidedEnemies(out var targets))
                    {
                        foreach (var target in targets)
                        {
                            Attack(target);
                        }
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
        }
        
        private void Rotate()
        {
            Rotation += Vector3.back * RotateSpeed * Time.deltaTime;
            _view.UpdateRotate(Rotation);
        }

        private void FadeAndActive(bool flag)
        {
            if (flag)
                _view.SetActive(true);
            
            var startValue = flag ? 0 : 1;
            var endValue = flag ? 1 : 0;

            if (_view.Model.TryGetComponent<SpriteRenderer>(out var component))
            {
                component.color = new Color(1, 1, 1, startValue);
                component.DOFade(endValue, 1f).OnComplete(() =>
                {
                    _view.SetActive(flag);
                });
            }
        }

    }
}
