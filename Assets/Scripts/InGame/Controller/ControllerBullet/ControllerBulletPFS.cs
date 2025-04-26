using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.InGame.Controller.ControllerBullet
{
    public class ControllerBulletPFS : ControllerBullet
    {
        private readonly ViewBulletPFC _view;

        public ControllerBulletPFS(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPFC>())
        {
            _view = (ViewBulletPFC)viewBullet;
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null)
        {
        }

        public override async UniTaskVoid Shot(Vector2 position, HashSet<IDamageable> nonTargets = null)
        {
            if (TryCopyBullet(out var bullet,  "PFC"))
                bullet.Shot(position, nonTargets).Forget();
        }
    }
}
