using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using UnityEngine.Events;

namespace ETD.Scripts.Manager
{
    public class StageManager : Singleton<StageManager>
    {
        public UnityAction<StageType, int> onBindChangeStageType;
        public UnityAction<StageType, int> onBindStartStage;
        public UnityAction<StageType> onBindStageClear;
        public UnityAction<StageType, bool> onBindStageEnd;
        public UnityAction onBindShowStageClearView;

        public StageType PrevPlayingStageType => _prevPlayingStageType;
        public StageType PlayingStageType => _playingStageType;
        public int PlayingStageLevel => _playingStageLevel;
        
        private CancellationTokenSource _cts;
        private StageType _playingStageType;
        private int _playingStageLevel;

        private StageType _prevPlayingStageType;
        private int _prevPlayingStageLevel;
        
        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
            ChangeStage(StageType.Normal);
        }
        
        #region NormalStage
        public void MoveToNormalStageLevel(int stageLevel, UnityAction fadeOutCallback = null, UnityAction fadeInCallback = null)
        {
            var controllerToastMessage = ControllerCanvas.Get<ControllerCanvasToastMessage>();
            controllerToastMessage.ShowFadeOutIn(() =>
            {
                EnemyManager.Instance.Clear();
                StartStage(StageType.Normal, stageLevel, false).Forget();
                fadeOutCallback?.Invoke();
            }, fadeInCallback);
        }
        #endregion

        public void  ChangeStage(StageType stageType, int param0 = 0)
        {
            _prevPlayingStageType = _playingStageType;
            if (stageType == StageType.Normal)
            {
                if (_prevPlayingStageType is StageType.GoldDungeon or StageType.DiaDungeon or StageType.EnhanceDungeon)
                {
                    var unlockType = _prevPlayingStageType switch
                    {
                        StageType.GoldDungeon => UnlockType.GoldDungeon,
                        StageType.DiaDungeon => UnlockType.DiaDungeon,
                        StageType.EnhanceDungeon => UnlockType.EnhanceDungeon,
                        _ => UnlockType.None
                    };
                    if (unlockType == UnlockType.None) return;
                    DataController.Instance.shop.TryUnlockNewPackage(unlockType, _playingStageLevel);
                }
            }
            
            _playingStageLevel = GetStartingStageLevel(stageType);
            _playingStageType = stageType;
            
            onBindChangeStageType?.Invoke(stageType, param0);
            StartStage(_playingStageType, _playingStageLevel, false).Forget();
        }

        private async UniTaskVoid StartStage(StageType stageType, int level, bool isImmediate)
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            
            if (stageType == StageType.Normal)
                DataController.Instance.stage.SaveStageLevelData(level);
            
            var delay = isImmediate ? 1 : 1000;
            await UniTask.Delay(delay, false, PlayerLoopTiming.Update, _cts.Token);
            onBindStartStage?.Invoke(stageType, level);
        }

        public void StageClear()
        {
            var stageType = _playingStageType;
            var isImmediate = false;

            switch (stageType)
            {
                case StageType.GoldDungeon:
                case StageType.DiaDungeon:
                case StageType.DarkDiaDungeon:
                case StageType.GuildRaidDungeon:
                    isImmediate = true;
                    break;
                case StageType.EnhanceDungeon:
                    onBindStageClear?.Invoke(stageType);
                    return;
            }

            _playingStageLevel = GetNextStageLevel(stageType);
            onBindStageClear?.Invoke(stageType);
            
            StartStage(stageType, _playingStageLevel, isImmediate).Forget();
        }
        
        public Dictionary<GoodType, double> GetRewardEachEnemy()
        {
            var rewardDic = _playingStageType switch
            {
                StageType.Normal => DataController.Instance.stage.CurrStageEachEnemyRewardDictionary,
                _ => DataController.Instance.dungeon.currStageEachEnemyRewardDictionary
            };
            
            return rewardDic;
        }

        private int GetStartingStageLevel(StageType stageType)
        {
            return stageType switch
            {
                StageType.Normal => DataController.Instance.stage.currTotalLevel,
                StageType.EnhanceDungeon => (int)DataController.Instance.dungeon.GetMaxGoalCount(stageType),
                _ => 0
            };
        }

        private int GetNextStageLevel(StageType stageType)
        {
            return stageType switch
            {
                StageType.Normal => DataController.Instance.stage.NextLevel,
                _ => DataController.Instance.dungeon.GetNextLevel(stageType, _playingStageLevel)
            };
        }
    }
}