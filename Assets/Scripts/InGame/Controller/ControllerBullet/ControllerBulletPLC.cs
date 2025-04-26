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
    public class ControllerBulletPLC : ControllerBullet
    {
        public int FlameRate { get; set; } = 1;
        public float AttackCoefficient { get; set; } = DataController.Instance.elementalCombine.GetAttackCoefficient("PLC");
        private readonly ViewBulletPLC _view;
        
        public ControllerBulletPLC(CancellationTokenSource cts, Transform parent)
            : base(cts, parent,  View.View.Get<ViewBulletPLC>())
        {
            _view = (ViewBulletPLC)viewBullet; 
        }

        public override async UniTaskVoid Shot(IDamageable unit, IDamageable enemy, HashSet<IDamageable> nonTargets = null){ }

        public override async UniTaskVoid Shot(Vector2 basePosition, HashSet<IDamageable> nonTargets = null)
        {
            _view.SetActive(true);

            SetFlameRate(FlameRate);
            _view.HealingEffect.Play();
            
            AutoDotAttack().Forget();
            
            while (_view.isActiveAndEnabled)
            {
                if (IsDotAttackable())
                {
                    var increaseValue = DataController.Instance.player.MaxHp * AttackCoefficient;
                    DataController.Instance.player.CurrHp += increaseValue;
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
            }
            
            _view.HealingEffect.Stop();
        }
        
        private void SetFlameRate(int value)
        {
            if (_view.HealingEffect.HasInt("FlamethrowerRate"))
            {
                _view.HealingEffect.SetInt("FlamethrowerRate", value);
            }
        }
    }
}
