using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View;
using ETD.Scripts.InGame.View.ViewEnemy;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller
{
    public class ControllerMainUnit : IDamageable
    {
        public Vector2 Position { get; set; }
        public Transform[] ProjectorTransforms => _view.ProjectorTransforms;
        public bool IsActive => _view.isActiveAndEnabled;
        public View.View View => _view;
        public float ColliderRange => _view.ColliderRange;

        private readonly ViewMainUnit _view;
        private readonly CancellationTokenSource _cts;

        private readonly float _rotateSpeed = 25f;
        private readonly float _rightRotateSpeed = 9f;
        private Vector3 _rotate;
        private Vector3 _rightRotate;

        public ControllerMainUnit(CancellationTokenSource cts)
        {
            _cts = cts;
            _view = ETD.Scripts.InGame.View.View.Get<ViewMainUnit>();
            _view.SetActive(true);

            Position = Vector2.zero;

            DataController.Instance.player.OnBindDie += Die;
            
            MainTask().Forget();
        }

        private async UniTaskVoid MainTask()
        {
            while (true)
            {
                Rotate();
                await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
                UpdateView();
            }
        }

        public void Damaged(double damage, bool isCritical)
        {
            if (!GameManager.Instance.IsPlaying) return;

            var reduceValue = DataController.Instance.research.GetValue(ResearchType.IncreaseDefenceRate);
            damage *= (1 - reduceValue);
                
            if(!ControllerCanvasTest.IsUnitInvincibility)
                DataController.Instance.player.CurrHp -= damage;
            
            DataController.Instance.player.OnBindChangedHp?.Invoke();

            if (DataController.Instance.player.CurrHp <= 0)
                DataController.Instance.player.OnBindDie?.Invoke(StageManager.Instance.PlayingStageType);
        }

        private void Rotate()
        {
            _rotate += Vector3.forward * _rotateSpeed * Time.deltaTime;
            _rightRotate += Vector3.back * _rightRotateSpeed * Time.deltaTime;
        }

        private void UpdateView()
        {
            _view.Rotate(_rotate);
            _view.ProjectorParentRotate(_rightRotate);
        }

        private void Die(StageType type)
        {
            if (GameManager.Instance.IsPlaying)
            {
                if (type == StageType.Normal)
                    ControllerCanvas.Get<ControllerCanvasDefeat>().ShowDefeatView();
                else
                {
                    GameManager.Instance.Pause();
                    StageManager.Instance.onBindShowStageClearView?.Invoke();
                }
            }
        }
    }
}