using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletPDB : ControllerBullet
    {
        public int CircleCount { get; set; } = 2;
        
        private readonly ViewBulletPDC _view;
        
        public ControllerBulletPDB(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPDC>())
        {
            _view = (ViewBulletPDC)viewBullet; 
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            Key = "PDC";
            if (TryCopyBullet(out var bullet))
            {
                ((ControllerBulletPDC)bullet).CircleCount = CircleCount;
                
                bullet.Shot(position, nonTargets).Forget();
            }
        }
    }
}
