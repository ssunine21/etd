using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewEnemy;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerEnemy
{
    public class ControllerEnemyDiaBoss : ControllerEnemy
    {
        private readonly ViewEnemyDiaBoss _view;

        public ControllerEnemyDiaBoss(CancellationTokenSource cts)
            : base(cts,  InGame.View.View.Get<ViewEnemyDiaBoss>("Enemy"), EnemyType.DiaBoss)
        {
            _view = (ViewEnemyDiaBoss)viewEnemy;
            _view.Damageable = this;
            
            _view.SetActive(true);
            
            MainTask().Forget();
        }

        public sealed override async UniTaskVoid MainTask()
        {
            while (IsActive)
            {
                Rotate();
                UpdateView();
                
                if(!IsInView)
                    IsInView = Utility.IsInView(Position);
                
                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
            }

            IsInView = false;
        }

        public override void Damaged(double damage, bool isCritical)
        {
            base.Damaged(damage, isCritical);
            _view.Tr.DOKill();
            _view.Tr.DOShakePosition(0.06f, 0.1f);
        }
    }
}
