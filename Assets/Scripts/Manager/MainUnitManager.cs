using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller;
using ETD.Scripts.Interface;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.Manager
{
    public class MainUnitManager : Singleton<MainUnitManager>
    {
        public ControllerMainUnit MainUnitController { get; private set; }
        public List<ControllerProjector> ControllerProjectors;

        private CancellationTokenSource _cts;

        private float _rotateSpeed = 8f;
        private Vector3 _rotation;

        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
            MainUnitController = new ControllerMainUnit(_cts);
            ControllerProjectors = new List<ControllerProjector>();
            
            var projectorCount = DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseProjector);
            for (var i = 0; i < projectorCount + 1; ++i)
            {
                CreateProjector(i);
            }

            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseProjector] += TryCreateProjector;
            
            MainTask().Forget();
        }
        
        public IDamageable OverlapCircle(Vector2 position, float range)
        {
            if (IsInRange(position, range))
                return MainUnitController;
            return null;
        }
        
        private bool IsInRange(Vector2 position, float range)
        {
            var distance = Vector3.Distance(MainUnitController.Position, position);
            return distance <= range + MainUnitController.ColliderRange;
        }
        
        private async UniTaskVoid MainTask()
        {
            while (true)
            {
                RotateProjectorParent();
                await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
            }
        }

        private void TryCreateProjector(int index)
        {
            if(IsCreatableProjector(index))
                CreateProjector(index);
        }

        private bool IsCreatableProjector(int index)
        {
            return index < 3 && index >= ControllerProjectors.Count;
        }

        private void CreateProjector(int index)
        {
            if (index >= MainUnitController.ProjectorTransforms.Length) return;
            
            var parent = MainUnitController.ProjectorTransforms[index];
            var projector = new ControllerProjector(_cts, index);
            projector.SetParent(parent);
            
            ControllerProjectors.Add(projector);
            ControllerProjectors[index].SetPosition(parent.position);
        }

        private void RotateProjectorParent()
        {
            _rotation += Vector3.back * _rotateSpeed * Time.deltaTime;
        }
    }
}