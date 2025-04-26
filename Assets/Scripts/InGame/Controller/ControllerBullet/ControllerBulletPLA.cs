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
    public class ControllerBulletPLA : ControllerBullet
    {
        public int FlameRate { get; set; } = 2;
        public float AttackCoefficient { get; set; } = DataController.Instance.elementalCombine.GetAttackCoefficient("PLA");
        
        private readonly ViewBulletPLC _view;
        
        public ControllerBulletPLA(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPLC>())
        {
            _view = (ViewBulletPLC)viewBullet; 
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null) { }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet, "PLC"))
            {
                ((ControllerBulletPLC)bullet).FlameRate = FlameRate;
                ((ControllerBulletPLC)bullet).AttackCoefficient = AttackCoefficient;
                
                bullet.Shot(position, nonTargets).Forget();
            }
        }
    }
}
