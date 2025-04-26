using System;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasLab
    {
        private int _saveDelay;
        private float _currCreatedDarkDia;
        private DateTime _darkDiaChargeDateTime;
        
        private void InitStorage()
        {
            UpdateCreateStorage();
            UpdateSaveStorage();
            UpdateDarkDiaPerSecond();

            View.StorageSave.onClick.AddListener(SaveDarkDiaToStorage);

            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseCreateStorage] += UpdateCreateStorage;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseSaveStorage] += UpdateSaveStorage;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseDarkDiaPerSec] += UpdateDarkDiaPerSecond;

            DataController.Instance.good.OnBindChangeGood += goodType =>
            {
                if(goodType == GoodType.DarkDia) UpdateSaveStorage();
            };

            _darkDiaChargeDateTime = string.IsNullOrEmpty(DataController.Instance.research.darkDiaChargeTime) 
            ? ServerTime.Date
            : ServerTime.IsoStringToDateTime(DataController.Instance.research.darkDiaChargeTime);
            
            DataController.Instance.contentUnlock.OnBindInitUnlockDic[UnlockType.Research] += InitDarkDiaChargeDateTime;
            
            ChargeDarkDia().Forget();
        }

        private void InitDarkDiaChargeDateTime()
        {
            _darkDiaChargeDateTime = ServerTime.Date.AddSeconds(-500);
        }

        private async UniTaskVoid ChargeDarkDia()
        {
            UniTask.WaitUntil(() => ServerTime.IsInit);

            if (string.IsNullOrEmpty(DataController.Instance.research.darkDiaChargeTime))
                DataController.Instance.research.SetChargeTime(ServerTime.DateTimeToIsoString(ServerTime.Date));

            
            while (true)
            {
                var value = GetChargedDarkDia();
                _currCreatedDarkDia = value;
                
                await UniTask.Delay(1000);

                _saveDelay -= 1;
                UpdateSaveTime();
                UpdateCreateStorage();
            }
        }

        private float GetChargedDarkDia()
        {
            var timeSpan = ServerTime.UntilTimeToServerTime(_darkDiaChargeDateTime);
            var value = DataController.Instance.research.GetDarkDiaPerSec() * timeSpan.TotalSeconds;

            value = Mathf.Clamp((float)value, 0, DataController.Instance.research.GetMaxCreateStorage());
            return (float)value;
        }

        private void SaveDarkDiaToStorage()
        {
            if (_saveDelay > 0)
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotYet);
                return;
            }

            var value = _currCreatedDarkDia;
            _currCreatedDarkDia = 0;
            
            Get<ControllerCanvasToastMessage>().ShowLoading();
            DataController.Instance.good.Earn(GoodType.DarkDia, value);

            DataController.Instance.SaveBackendData((_) =>
            {
                _saveDelay = DataController.Instance.research.GetSaveTime();
                DataController.Instance.research.SetChargeTime(ServerTime.DateTimeToIsoString(ServerTime.Date));
                _darkDiaChargeDateTime = ServerTime.Date;
                
                UpdateCreateStorage();
                UpdateSaveStorage();
                UpdateSaveTime();
                
                GoodsEffectManager.Instance.ShowEffect(GoodType.DarkDia, Vector2.zero, View.ViewGoods[0], 10);
                
                Get<ControllerCanvasToastMessage>().CloseLoading();
            });

        }
        
        private void UpdateCreateStorage()
        {
            var currValue = (int)_currCreatedDarkDia;
            var maxValue = DataController.Instance.research.GetMaxCreateStorage();

            View.SetDarkDiaCreateText($"{currValue} <color=orange><size=80%>/{maxValue}</size></color>");
        }
        
        private void UpdateDarkDiaPerSecond()
        {
            var value = DataController.Instance.research.GetDarkDiaPerSec();
            var text = LocalizeManager.GetText(LocalizedTextType.CreateDarkDiaPerSec, value);

            View.SetDarkDiaPerSecText(text);
        }
        
        private void UpdateSaveStorage()
        {
            var currValue = DataController.Instance.good.GetValue(GoodType.DarkDia);
            var maxValue = DataController.Instance.research.GetMaxSaveStorage();

            var currValueText = currValue >= maxValue
                ? $"<color={Utility.GetRedColorToHex()}>{currValue.ToGoodString(GoodType.DarkDia)}</color>"
                : $"{currValue.ToGoodString(GoodType.DarkDia)}";
            var maxValueText = $" <color=orange><size=80%>/{maxValue}</size></color>";

            var isProtected = DataController.Instance.player.IsProtected();
            var protectPercent = DataController.Instance.raid.GetProtectedPercentage();
            var plunderPercent = DataController.Instance.raid.GetPlunderPercentage();
            var percentText = (isProtected ? 
                $"{protectPercent:P0}<color=green> (-{plunderPercent - protectPercent:P0})</color>" :
                $"{plunderPercent:P0}").Replace(" ", "");
            
            UpdateProtectTime();
            
            View
                .SetStorageSaveText($"{currValueText}{maxValueText}")
                .SetStorageDescText(LocalizeManager.GetText(LocalizedTextType.StorageDesc, percentText));
        }

        private void UpdateProtectTime()
        {
            var text = string.Empty;
            if(DataController.Instance.player.IsProtected())
            {
                var timeSpan = DataController.Instance.player.GetProtectRemainTimeSpan();
                var timeText = Utility.GetTimeStringToFromTotalSecond(timeSpan);
                text = $"<color=green>({LocalizeManager.GetText(LocalizedTextType.Protect)}) - {timeText}</color>";
            }
            View.SetProtectedText(text);
        }

        private void UpdateSaveTime()
        {
            _saveDelay = Mathf.Clamp(_saveDelay, 0, DataController.Instance.research.GetSaveTime());
            View.SetLockSaveButton(_saveDelay > 0);
            View.SetSaveTimeText($"{_saveDelay}s");
        }
    }
}