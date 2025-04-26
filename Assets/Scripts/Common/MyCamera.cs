using System;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Manager;
using UnityEngine;

namespace ETD.Scripts.Common
{
    public class MyCamera : Singleton<MyCamera>
    {
        private Transform _tr;

        private void Start()
        {
            _tr = GetComponent<Transform>();
            StageManager.Instance.onBindChangeStageType += UpdatePosition;
        }

        public override void Init(CancellationTokenSource cts)
        {
        }

        private void Move(Vector2 endValue)
        {
            var value = (Vector3)endValue;
            value.z = -10;
            
            _tr.DOMove(value, 1f).SetEase(Ease.InOutQuart).SetUpdate(true);
        }

        private void ResetPosition()
        {
            Move(Vector2.zero);
        }

        private void UpdatePosition(StageType stageType, int param0)
        {
            if(stageType is StageType.DiaDungeon or StageType.GuildRaidDungeon)
                Move(new Vector2(0, 1.6f));
            else
                ResetPosition();
        }
    }
}
